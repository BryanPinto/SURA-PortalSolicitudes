using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WebSolicitudes.Models;
using WebSolicitudes.WorkflowEngine;
using WebSolicitudes.EntityManager;
using WebSolicitudes.QuerySoa;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;

namespace WebSolicitudes.Controllers
{
    public class CasosController : Controller
    {
        //
        // GET: /Casos/

        public ActionResult Index()
        {
            return RedirectToAction("Creacion", "Casos");
        }



        //
        // GET: /Casos/Creacion

        public ActionResult Creacion(long? id)
        {
            #region Cargar campos
            ViewData["nroCaso"] = id;

            // Información solicitante
            ViewData["txtNombreSolicitante"] = "Nombre Apellido";
            ViewData["txtCorreoSolicitante"] = "correo@gmail.com";
            ViewData["txtFechaCreacion"] = DateTime.Today.ToString("dd/MM/yyyy");

            // Listas desplegables
            string listaUnidad = Util.ListarParametrica("P_Unidad", "NombreUnidad");
            ViewData["txtUnidadRecurso"] = listaUnidad;

            #endregion

            return View();
        }



        //
        // POST: /Casos/Creacion

        [HttpPost]
        public ActionResult Creacion(FormCollection collection, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                #region Leer variables

                // Cabecera
                int idUsuarioCreador = 1;
                string txtNombreSolicitante = collection["txtNombreSolicitante"] ?? string.Empty;
                string txtCorreoSolicitante = collection["txtCorreoSolicitante"] ?? string.Empty;

                // Datos solicitud
                List<Recurso> recursos = new List<Recurso>();
                List<string> filas = collection.AllKeys.Where(d => d.StartsWith("txtCantidad")).ToList();

                // Recorrer filas de la tabla
                foreach (string fila in filas)
                {
                    // Extraer nro de fila
                    int nroFila = int.Parse(Regex.Match(fila, @"\d+").Value);

                    // Guardar variables
                    string nombre = collection["txtNombreRecurso" + nroFila];
                    int unidad = int.Parse(collection["txtUnidadRecurso" + nroFila]);
                    int cantidad = int.Parse(collection["txtCantidadRecurso" + nroFila]);

                    // Crear recurso y agregar a colección
                    recursos.Add(new Recurso(nombre, unidad, cantidad));
                }

                // Leer archivo
                string campoDocumento = "txtDocumento";
                string path = Path.GetTempPath();
                string filename = "archivo_temporal";
                string nombreOriginal = Request.Files[campoDocumento].FileName;

                string ext = Path.GetExtension(Request.Files[campoDocumento].FileName);
                Request.Files[campoDocumento].SaveAs(Path.Combine(path, filename + ext));
                string txtDocumento = Util.ConvertirABase64(path + filename + ext);

                // Obtener usuario Bizagi
                string usuarioBizagi = System.Configuration.ConfigurationManager.AppSettings["usuarioBizagi"].ToString();
                string dominioBizagi = System.Configuration.ConfigurationManager.AppSettings["dominioBizagi"].ToString();

                #endregion

                #region Armar XML
                string nombreProceso = "AnalisisDeRecursos";

                // Parte de recursos
                string xmlRecursos = string.Empty;
                foreach (Recurso recurso in recursos)
                {
                    xmlRecursos += @"
                        <M_RecursosInvolucrados>
                            <Nombre>" + recurso.Nombre + @"</Nombre>
                            <Cantidad>" + recurso.Cantidad + @"</Cantidad>
                            <Unidad>" + recurso.Unidad + @"</Unidad>
                       </M_RecursosInvolucrados>";
                }

                // XML General
                string xmlCreacion = @"
                <BizAgiWSParam>
                    <userName>" + usuarioBizagi + @"</userName>
                    <domain>" + dominioBizagi + @"</domain>
                    <Cases>
                        <Case>
                            <Process>" + nombreProceso + @"</Process>
                            <Entities>
                                <AnalisisDeRecursos>
                                    <Usuariocreador>" + idUsuarioCreador + @"</Usuariocreador>
                                    <Documento>
                                        <File fileName='" + nombreOriginal + @"'>" + txtDocumento + @"</File>
                                    </Documento>
                                    <RecursosInvolucrados>
                                        " + xmlRecursos + @"
                                    </RecursosInvolucrados>
                                </AnalisisDeRecursos>
                            </Entities>
                        </Case>
                    </Cases>
                </BizAgiWSParam>
                ";

                xmlCreacion = xmlCreacion.Replace("\n", "");
                xmlCreacion = xmlCreacion.Replace("\t", "");
                xmlCreacion = xmlCreacion.Replace("\r", "");

                Util.EscribirLog("Creacion", "XML Creación", xmlCreacion);
                #endregion

                #region Respuesta Bizagi
                XmlDocument doc = new XmlDocument();
                string respuesta = string.Empty;

                // Abrir conexión a servicio web
                using (WorkflowEngineSOASoapClient workflowEngine = new WorkflowEngineSOASoapClient())
                {
                    // Llamar a Bizagi
                    respuesta = workflowEngine.createCasesAsString(xmlCreacion);
                    Util.EscribirLog("Creacion", "XML Respuesta creación", respuesta);
                }

                // Convertir a XML
                doc.LoadXml(respuesta);

                string nroCaso = doc.SelectSingleNode("/processes/process/processId").InnerText;

                // Si hubo error
                if (nroCaso == "0")
                {
                    string mensajeError = doc.SelectSingleNode("/processes/process/processError/errorMessage/Entities/ErrorMessage").InnerText;
                    Util.EscribirLog("Creacion", "Error creación", mensajeError);
                }

                // Mandar mensaje a vista
                return RedirectToAction("Creacion", new { id = nroCaso });

                #endregion

            }
            catch (Exception ex)
            {
                Util.EscribirLog("Creacion", "Error Creación", ex.Message);
                return RedirectToAction("Creacion", new { estado = 0 });
            }
            return View();
        }



        //
        // GET: /Casos/Resumen/NroCaso

        public ActionResult Resumen(int id)
        {
            try
            {

                #region XML Búsqueda

                string xmlBusqueda = @"
                <BizAgiWSParam>
	                <CaseInfo>
		                <CaseNumber>" + id + @"</CaseNumber>
	                </CaseInfo>
	                <XPaths>
                        <XPath XPath=""AnalisisDeRecursos.Usuariocreador.fullName""/>
		                <XPath XPath=""AnalisisDeRecursos.Usuariocreador.contactEmail""/>
                        <XPath XPath=""AnalisisDeRecursos.Fechadecreacion""/>
                        <XPath XPath=""AnalisisDeRecursos.Documento""/>

		                <XPath XPath=""AnalisisDeRecursos.RecursosInvolucrados.Nombre""/>
                        <XPath XPath=""AnalisisDeRecursos.RecursosInvolucrados.Cantidad""/>
                        <XPath XPath=""AnalisisDeRecursos.RecursosInvolucrados.Unidad.NombreUnidad""/>
                    </XPaths>
                </BizAgiWSParam>
            ";

                xmlBusqueda = xmlBusqueda.Replace("\n", "");
                xmlBusqueda = xmlBusqueda.Replace("\t", "");
                xmlBusqueda = xmlBusqueda.Replace("\r", "");

                Util.EscribirLog("Busqueda", "XML Forma de resumen: Búsqueda", xmlBusqueda);

                #endregion

                XmlDocument doc = new XmlDocument();
                string respuestaBusqueda = string.Empty;

                using (EntityManagerSOASoapClient entityManager = new EntityManagerSOASoapClient())
                {
                    respuestaBusqueda = entityManager.getCaseDataUsingXPathsAsString(xmlBusqueda);
                    Util.EscribirLog("Busqueda", "XML Forma de resumen: Respuesta búsqueda", xmlBusqueda);
                }

                // Convertir respuesta a XML

                doc.LoadXml(respuestaBusqueda);

                // Seleccionar campos
                string txtNombreSolicitante = doc.SelectSingleNode("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.Usuariocreador.fullName']").InnerText;
                string txtCorreoSolicitante = doc.SelectSingleNode("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.Usuariocreador.contactEmail']").InnerText;
                string txtFechaCreacion = doc.SelectSingleNode("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.Fechadecreacion']").InnerText;

                string documentoBase64 = string.Empty;
                string documentoNombre = string.Empty;
                bool tieneArchivo = doc.SelectSingleNode("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.Documento']/Items/Item") != null;
                if (tieneArchivo)
                {
                    documentoBase64 = doc.SelectSingleNode("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.Documento']/Items/Item").InnerText;
                    documentoNombre = doc.SelectSingleNode("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.Documento']/Items/Item").Attributes["FileName"].InnerText;
                }
                XmlNodeList recursosNombre = doc.SelectNodes("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.RecursosInvolucrados.Nombre']/Items/Item");
                XmlNodeList recursosCantidad = doc.SelectNodes("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.RecursosInvolucrados.Cantidad']/Items/Item");
                XmlNodeList recursosUnidad = doc.SelectNodes("/BizAgiWSResponse/XPath[@XPath='AnalisisDeRecursos.RecursosInvolucrados.Unidad.NombreUnidad']/Items/Item");

                // Armar tabla
                string tabla = string.Empty;
                for (int i = 0; i < recursosNombre.Count; i++)
                {
                    int nroFila = i + 1;
                    tabla += @"
                    <tr id='row" + nroFila + @"'>
                        <td>
                            <input type='text' value='" + recursosNombre[i].InnerText + @"' name='txtNombreRecurso" + nroFila + @"' id='txtNombreRecurso" + nroFila + @"' placeholder='Nombre' class='form-control' readOnly='readOnly' />
                        </td>
                        <td>
                            <input type='text' value='" + recursosUnidad[i].InnerText + @"' name='txtUnidadRecurso" + nroFila + @"' id='txtUnidadRecurso" + nroFila + @"' placeholder='Unidad' class='form-control' readOnly='readOnly' />
                        </td>
                        <td>
                            <input type='text' value='" + recursosCantidad[i].InnerText + @"' name='txtCantidadRecurso" + nroFila + @"' id='txtCantidadRecurso" + nroFila + @"' placeholder='Cantidad' class='form-control' readOnly='readOnly' />
                        </td>
                    </tr>
                    ";
                }
                tabla = tabla.Replace("\n", "");
                tabla = tabla.Replace("\t", "");
                tabla = tabla.Replace("\r", "");

                #region Cargar campos

                ViewData["txtNroCaso"] = id;

                ViewData["txtNombreSolicitante"] = txtNombreSolicitante;
                ViewData["txtCorreoSolicitante"] = txtCorreoSolicitante;
                ViewData["txtFechaCreacion"] = txtFechaCreacion;

                ViewData["txtTabla"] = tabla;

                if (tieneArchivo)
                {
                    ViewData["txtDocumento"] = @"
                    <a download='" + documentoNombre + @"' href='data:application/octet-stream;charset=utf-16le;base64," + documentoBase64 + @"' class='btn btn-primary btn-md'>
                        <span class='glyphicon glyphicon-save'></span> Descargar " + documentoNombre + @"
                    </a>
                    ";
                }

                #endregion

            }
            catch (Exception ex)
            {
                Util.EscribirLog("Creacion", "Error Creación", ex.Message);
                return RedirectToAction("Creacion", new { estado = 0 });
            }
            return View();
        }


        //
        // GET: /Casos/BusquedaCasos

        [HttpPost]
        public string BusquedaCasos(FormCollection collection)
        {
            string datosJSON = string.Empty;
            try
            {
                #region Agregar filtros y buscar
                // Variables
                string txtFechaDesde = collection["txtFechaDesde"] ?? string.Empty;
                string txtFechaHasta = collection["txtFechaHasta"] ?? string.Empty;
                string txtNroCaso = collection["txtNroCaso"] ?? string.Empty;
                string txtEstado = collection["txtEstadoCaso"];
                List<Actividad> actividades = new List<Actividad>();

                // Fecha inicio
                DateTime? fechaInicio = null;
                if (txtFechaDesde != string.Empty)
                    fechaInicio = DateTime.ParseExact(txtFechaDesde, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Fecha término
                DateTime? fechaTermino = null;
                if (txtFechaHasta != string.Empty)
                    fechaTermino = DateTime.ParseExact(txtFechaHasta, "yyyy-MM-dd", CultureInfo.InvariantCulture);


                // Estado
                int? estado = null;
                if (txtEstado != "0")
                    estado = int.Parse(txtEstado);


                #endregion

                #region Queries

                QueryFormSOASoapClient servicioQuery = new QueryFormSOASoapClient();

                int cantidadCasos = 20;
                string respuestaCasos = "";

                //Crear XML de consulta de casos (idWfClass = id de proceso)
                string queryCasos = @"
                <BizAgiWSParam>
                  <QueryParams>
                      <Internals>
                          <Internal Name='ProcessState' Include='true'>Running</Internal>
                          <Internal Name='ProcessState' Include='true'>Completed</Internal>
                          <Internal Name='idWfClass' Include='true'>9</Internal>
                          <Internal Name='idTask' Include='true'></Internal>
                      </Internals>
                      <XPaths>
                          <XPath Path='AnalisisDeRecursos.Fechadecreacion' Include='true'>";
                            if (txtFechaDesde != string.Empty)
                            {
                                queryCasos += @"<From>" + fechaInicio + @"</From>";
                            }
                            else
                            {
                                queryCasos += @"<From>01/01/1900</From>";
                            }
                            if (txtFechaHasta != string.Empty)
                            {
                                queryCasos += @"<To>" + fechaTermino + @"</To>";
                            }
                            queryCasos += @"        
                          </XPath>
                      </XPaths>
                  </QueryParams>
                  <Parameters>
                      <Parameter Name ='pag'>1</Parameter>
                       <Parameter Name='PageSize'>" + cantidadCasos + @"</Parameter>
                    </Parameters>
                </BizAgiWSParam>";

                queryCasos = queryCasos.Replace("\n", "");
                queryCasos = queryCasos.Replace("\t", "");
                queryCasos = queryCasos.Replace("\r", "");

                respuestaCasos = servicioQuery.QueryCasesAsString(queryCasos);
                respuestaCasos = respuestaCasos.Replace("\n", "");
                respuestaCasos = respuestaCasos.Replace("\t", "");
                respuestaCasos = respuestaCasos.Replace("\r", "");

                //Transformar respuesta STRING de Bizagi a XML para poder recorrer los nodos
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(respuestaCasos);
                XmlNodeList rows = doc.GetElementsByTagName("Row");

                //Recorrer rows para obtener las actividades de cada caso
                XmlNodeList rows2 = doc.SelectNodes("BizAgiWSResponse/Results/Tables/TaskTable/Rows/Row");
                actividades = new List<Actividad>();
                if (rows2 != null)
                {
                    foreach(XmlNode row in rows2)
                    {
                        var actividadActual = "";
                        var casoActual = 0;
                        if (row.SelectNodes("Column[@Name='CurrentTask']")[0] != null && row.SelectNodes("Column[@Name='CurrentTask']")[0].InnerText != string.Empty)
                        {
                            actividadActual = row.SelectNodes("Column[@Name='CurrentTask']")[0].InnerText;
                        }
                        if (row.SelectNodes("Column[@Name='idCase']")[0] != null && row.SelectNodes("Column[@Name='idCase']")[0].InnerText != string.Empty)
                        {
                            var idCaso = row.SelectNodes("Column[@Name='idCase']")[0].InnerText;
                            if(idCaso != "0")
                            {
                                casoActual = Convert.ToInt32(idCaso);
                            }
                        }
                        Actividad actividad = new Actividad();
                        actividad.IdCase = casoActual;
                        actividad.NombreActividad = actividadActual;
                        actividades.Add(actividad);
                    }
                }

                #endregion

                //#region Crear JSON
                List<List<string>> registros = new List<List<string>>();
                if (rows != null)
                {
                    foreach (XmlNode row in rows)
                    {
                        List<string> fila = new List<string>();
                        Actividad actividad = new Actividad();
                        bool valido = false;
                        //OBTENER NUMERO DE CASO
                        var numCaso = "";
                        if (row.SelectNodes("Column[@Name='IDCASE']")[0] != null && row.SelectNodes("Column[@Name='E_ANALISISDE_FECHADECREACI']")[0].InnerText != string.Empty)
                        {
                            numCaso = row.SelectNodes("Column[@Name='IDCASE']")[0].InnerText;
                            if (txtNroCaso == string.Empty)
                            {
                                valido = true;
                                fila.Add(numCaso);
                            }
                            else if (txtNroCaso != string.Empty && txtNroCaso == numCaso)
                            {
                                valido = true;
                                fila.Add(numCaso);
                            }
                        }

                        //OBTENER ESTADO DEL CASO
                        if (row.SelectNodes("Column[@Name='IDCASESTATE']")[0] != null)
                        {
                            var estadoCaso = row.SelectNodes("Column[@Name='IDCASESTATE']")[0].InnerText;
                            var estadoTexto = "";
                            if (Convert.ToUInt32(estadoCaso) == 1)
                            {
                                estadoTexto = "Iniciado";
                            }
                            else if (Convert.ToUInt32(estadoCaso) == 2)
                            {
                                estadoTexto = "En proceso";
                            }
                            else if (Convert.ToInt32(estadoCaso) == 5)
                            {
                                estadoTexto = "Completado";
                            }
                            fila.Add(estadoTexto);
                        }

                        

                        //OBTENER FECHA 
                        if (row.SelectNodes("Column[@Name='E_ANALISISDE_FECHADECREACI']")[0] != null && row.SelectNodes("Column[@Name='E_ANALISISDE_FECHADECREACI']")[0].InnerText != string.Empty)
                        {
                            var fechaSolicitud = row.SelectNodes("Column[@Name='E_ANALISISDE_FECHADECREACI']")[0].InnerText;
                            DateTime fecha = Convert.ToDateTime(fechaSolicitud);
                            var fechaFinal = fecha.ToString("dd-MM-yyyy");
                            fila.Add(fechaFinal);
                        }

                        //OBTENER ACTIVIDAD ACTUAL (SE RECORREN POR SI UN CASO ESTA EN MAS DE UNA ACTIVIDAD A LA VEZ)
                        var filas = "";
                        foreach (Actividad act in actividades.Where(d => d.IdCase.ToString() == numCaso).ToList())
                        {
                            filas += act.NombreActividad + "<br/>";
                        }
                        fila.Add(filas);

                        fila.Add(@"<a href='" + Url.Action("Resumen", "Casos", new { id = numCaso }) + @"' class='btn btn-default btn-md center-block'>Resumen caso</a>");

                        // Agregar a lista FORMA CORRECTA
                        if (valido)
                            registros.Add(fila);

                    }
                    datosJSON = JsonConvert.SerializeObject(registros);

                }
            }
            catch (Exception ex)
            {
                Util.EscribirLog("Casos", "Búsqueda casos", ex.Message);
            }
            return (datosJSON);
        }
    }
}
