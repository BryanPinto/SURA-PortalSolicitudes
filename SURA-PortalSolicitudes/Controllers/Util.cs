using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using WebSolicitudes.EntityManager;

namespace WebSolicitudes.Controllers
{
    public class Util
    {
        /// <summary>
        /// Método para escribir un log
        /// Se guardar dentro de un CSV con nombre LogPortal.CSV dentro de la carpeta del proyecto
        /// </summary>
        /// <param name="proceso">Nombre del proceso</param>
        /// <param name="metodo">Método actual</param>
        /// <param name="mensaje">Mensaje que se va a guardar</param>
        public static void EscribirLog(string proceso, string metodo, string mensaje)
        {
            try
            {
                // Crear CSV
                string rutaLog = HttpRuntime.AppDomainAppPath;
                string nombreArchivo = "LogPortal.csv";
                string rutaCompleta = rutaLog + "LogPortal.csv";
                var csv = new StringBuilder();

                // Revisar si tiene cabecera
                string primeraLinea = string.Empty;
                bool existeArchivo = System.IO.File.Exists(rutaCompleta);
                if (existeArchivo)
                    primeraLinea = System.IO.File.ReadLines(rutaCompleta).FirstOrDefault();
                if (!existeArchivo || (existeArchivo && (primeraLinea == null || primeraLinea == string.Empty)))
                {
                    string cabecera =
                        string.Format("{0};{1};{2};{3};{4}"
                        , "Fecha"
                        , "Hora"
                        , "Proceso"
                        , "Método"
                        , "Mensaje"
                        );
                    csv.Append(cabecera);
                    System.IO.File.AppendAllText(rutaCompleta, csv.ToString());
                    csv.Clear();
                }

                // Crear línea
                string nuevaLinea = Environment.NewLine +
                    string.Format("{0};{1};{2};{3};{4}"
                    , "\"" + DateTime.Now.ToShortDateString() + "\""
                    , "\"" + DateTime.Now.ToShortTimeString() + "\""
                    , "\"" + proceso + "\""
                    , "\"" + metodo + "\""
                    , "\"" + mensaje + "\""
                    );

                // Agregar a archivo
                csv.Append(nuevaLinea);
                System.IO.File.AppendAllText(rutaCompleta, csv.ToString());
                csv.Clear();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Método para crear listas desplegables desde una paramétrica de Bizagi
        /// </summary>
        /// <param name="tabla">Nombre de tabla paramétrica</param>
        /// <param name="campoVisual">Campo que se va a mostrar como opción</param>
        /// <returns></returns>
        public static string ListarParametrica(string tabla, string campoVisual)
        {
            string lista = string.Empty;

            try
            {
                // XML Búsqueda
                string xmlGetEntities = @"
                    <BizAgiWSParam>
                        <EntityData>
                            <EntityName>" + tabla + @"</EntityName>
                            <Filters>
                                <![CDATA[dsbl" + tabla + @" = " + false + @"]]>
                            </Filters>
                        </EntityData>
                    </BizAgiWSParam>
                ";

                // Abrir conexión a servicio web
                using (EntityManagerSOASoapClient entityManager = new EntityManagerSOASoapClient())
                {
                    // Buscar en Bizagi
                    string respuesta = entityManager.getEntitiesAsString(xmlGetEntities);

                    // Convertir a XML
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(respuesta);

                    // Recorrer los resultados
                    foreach (XmlNode item in doc.SelectNodes("/BizAgiWSResponse/Entities/" + tabla))
                    {
                        // Obtener campos
                        string id = item.Attributes["key"].Value;
                        string campo = item.SelectSingleNode(campoVisual).InnerText;

                        // Crear opción
                        lista += "<option value='" + id + @"'>" + campo + @"</option>";
                    }
                }
            }
            catch (Exception ex)
            {
                EscribirLog("Util", "listarParametricas", ex.Message);
            }

            return (lista);
        }


        #region Conversión base64
        
        /// 
        /// Cabezera para descargar archivo base64
        /// <a download href="data:application/pdf;base64,JVBER...>Descargar</a>
        ///

        /// <summary>
        /// Convierte un archivo a base64
        /// Devuelve la URI del archivo
        /// </summary>
        /// <param name="archivo">Ruta con nombre del archivo</param>
        /// <returns>File URI</returns>
        public static string ConvertirABase64(string file)
        {
            try
            {
                byte[] array = File.ReadAllBytes(file);
                string res = Convert.ToBase64String(array);
                Console.WriteLine(res);
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Convierte una cadena de base64 a un archivo
        /// El archivo queda en la carpeta de ejecución del programa (debug)
        /// </summary>
        /// <param name="cadena">Cadena en base64</param>
        /// <param name="nombre">Nombre archivo de salida</param>
        /// <param name="extension">Extensión archivo de salida</param>
        /// <returns>true si la conversión fue correcta</returns>
        public static Boolean ConvertirDesdeBase64(string cadena, string nombre, string extension)
        {
            try
            {
                if (cadena == null || cadena == string.Empty)
                    throw new Exception("Cadena vacía");
                else if (nombre == null || nombre == string.Empty)
                    throw new Exception("Nombre vacío");
                else if (extension == null || extension == string.Empty)
                    throw new Exception("Extensión vacía");
                else
                {
                    string salida = nombre + "." + extension;
                    byte[] resultadoFinal = Convert.FromBase64String(cadena);
                    File.WriteAllBytes(salida, resultadoFinal);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion

    }
}