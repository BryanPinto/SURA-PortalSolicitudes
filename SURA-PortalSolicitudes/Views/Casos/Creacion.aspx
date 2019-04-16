<%@ Page Title="" Language="C#" MasterPageFile="~/Views/DisenoBootstrap3.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <title>Creación de casos</title>
    <script>
        $(document).ready(function () {
            // Prevenir envío al presionar enter
            $(window).keydown(function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();
                    return false;
                }
            });

            // Prevenir doble submit
            $("#formSolicitud").submit(function () {
                var form = $("#formSolicitud");
                form.validate();
                if (form.valid())
                    $('input[type=submit]', this).attr('disabled', 'disabled');
            });

            // Función para agregar fila
            var nroFila = 1;
            jQuery.agregarFila = function () {
                nroFila++;
                var fila = $(`
                    <tr id="row`+ nroFila + `">
                        <td>
                            <input type="text" name="txtNombreRecurso`+ nroFila + `" id="txtNombreRecurso` + nroFila + `" placeholder="Nombre" class="form-control" />
                        </td>
                        <td>
                            <select class="form-control" name="txtUnidadRecurso`+ nroFila + `" id="txtUnidadRecurso` + nroFila + `">
                                <option value="0">-- Seleccione opción --</option>
                                <%= ViewData["txtUnidadRecurso"] %>
                            </select>
                        </td>
                        <td>
                            <input type="text" name="txtCantidadRecurso`+ nroFila + `" id="txtCantidadRecurso` + nroFila + `" placeholder="Cantidad" class="form-control" />
                        </td>
                        <td>
                            <button type="button" id="btnEliminar" class="btn btn-danger center-block">
                                <i class="icon-user glyphicon glyphicon-trash"></i>
                            </button>
                        </td>
                    </tr>
                    `);

                fila.hide();

                $('#tablaRecursos tbody tr:last').after(fila);

                // Validaciones y reglas
                $("#txtNombreRecurso" + nroFila).rules("add", {
                    required: true
                });
                $("#txtUnidadRecurso" + nroFila).rules("add", {
                    listaDesplegable: true
                });
                $("#txtCantidadRecurso" + nroFila).rules("add", {
                    required: true
                });

                fila.fadeIn("slow");
                return nroFila;
            }

            // Agregar fila
            $("#tablaRecursos").on('click', '#btnAgregar', function () {
                $.agregarFila();
            });

            // Eliminar fila
            $("#tablaRecursos").on('click', '#btnEliminar', function () {
                $(this).parent().parent().hide("slow", function () {
                    $(this).remove();
                });
            });

            // Mostrar mensaje de creación o error
            if ("<%= ViewData["nroCaso"] %>" != "" && "<%= ViewData["nroCaso"] %>" != "0")
                $("#msgCreada").fadeIn("slow");
            else if ("<%= ViewData["nroCaso"] %>" == "0")
                $("#msgError").fadeIn("slow");
        });
    </script>
    <script>
        ///                    ///
        ///    VALIDACIONES    ///
        ///                    ///

        // Propiedades para bootstrap
        $.validator.setDefaults({
            highlight: function (element) {
                $(element).closest('.form-group').addClass('has-error');
            },
            unhighlight: function (element) {
                $(element).closest('.form-group').removeClass('has-error');
            },
            errorElement: 'span',
            errorClass: 'help-block',
            errorPlacement: function (error, element) {
                if (element.parent('.input-group').length) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element);
                }
            }
        });

        // Cambiar mensajes por defecto
        jQuery.extend(jQuery.validator.messages, {
            required: "Este campo es requerido.",
            remote: "Este campo necesita correçción.",
            email: "Ingrese un correo válido.",
            url: "Ingrese una URL válida.",
            date: "Ingrese una fecha válida.",
            dateISO: "Ingrese una fecha válida.",
            number: "Ingrese un número válido.",
            digits: "Ingrese solo dígitos.",
            creditcard: "Ingrese un tarjeta de crédito válida.",
            equalTo: "Repita el mismo campo.",
            accept: "Ingrese un archivo con una extensión válida (PDF, DOC, DOCX, JPG o PNG).",
            extension: "Ingrese un archivo con una extensión válida (PDF, DOC, DOCX, JPG o PNG).",
            maxlength: jQuery.validator.format("Ingrese hasta {0} caracteres."),
            minlength: jQuery.validator.format("Ingrese mínimo {0} caracteres."),
            rangelength: jQuery.validator.format("Ingrese un valor en un rango entre {0} y {1}."),
            range: jQuery.validator.format("Ingrese un valor entre {0} y {1}."),
            max: jQuery.validator.format("Ingrese un valor igual o menor a {0}."),
            min: jQuery.validator.format("Ingrese un valor mayor o igual a {0}.")
        });

        // Validaciones
        $(document).ready(function () {
            $("#formSolicitud").validate({
                ignore: ":not(:visible)",
                onfocusout: function (element) {
                    $(element).valid();
                },
                rules: {
                    txtNombreRecurso1: "required",
                    txtCantidadRecurso1: "required",
                    txtUnidadRecurso1: {
                        listaDesplegable: true
                    },
                    //txtDocumento: {
                    //    required: true,
                    //    extension: "docx|doc|pdf|jpg|jpeg|png",
                    //    filesize: 102400
                    //},
                },
                messages: {

                }
            });

        });

        // Fecha mayor a hoy
        $.validator.addMethod("fechaMayorHoy", function (value, element) {
            var hoy = new Date();
            var fechaContrato = new Date(value);
            hoy.setHours(0, 0, 0, 0);
            fechaContrato.setHours(0, 0, 0, 0);
            fechaContrato.setDate(fechaContrato.getDate() + 1);

            if (fechaContrato <= hoy)
                return true;
            return false;
        }, "La fecha no puede ser mayor al día de hoy");

        // Seleccionar tipo
        $.validator.addMethod("listaDesplegable", function (value, element) {
            return (value != "" && value !== "0");
        }, "Seleccione una opción");
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Creación de caso</h2>

    <form action="<%: Url.Content("~/Casos/Creacion") %>" method="post" enctype="multipart/form-data" id="formSolicitud">
        <%--Mensajes de creación--%>
        <div>
            <div class="alert alert-success" style="display: none;" id="msgCreada">
                <strong>Solicitud creada</strong> Su solicitud ha sido ingresada con éxito con número <strong><a href="<%: Url.Content("~/Casos/Resumen/"+ViewData["nroCaso"]) %>">caso <%= ViewData["nroCaso"] %></a></strong>.
            </div>
            <div class="alert alert-danger" style="display: none;" id="msgError">
                <strong>Error</strong> Hubo un error al intentar ingresar la solicitud. Comuníquese con un administrador.
            </div>
        </div>
        
        <%--CABECERA--%>
        <div class="panel panel-primary">
            <div class="panel-heading">Información solicitante</div>
            <div class="panel-body">
                <div class="row">
                    <fieldset class="form-group col-md-4">
                        <label for="txtNombreSolicitante">Nombre solicitante</label>
                        <input type="text" class="form-control" id="txtNombreSolicitante" name="txtNombreSolicitante" readonly="readonly" placeholder="Nombre solicitante" value="<%= ViewData["txtNombreSolicitante"] %>" />
                    </fieldset>
                    <fieldset class="form-group col-md-4">
                        <label for="txtUnidadResponsable">Correo solicitante</label>
                        <input type="text" class="form-control" id="txtCorreoSolicitante" name="txtCorreoSolicitante" placeholder="Correo solicitante" readonly="readonly" value="<%= ViewData["txtCorreoSolicitante"] %>" />
                    </fieldset>
                    <fieldset class="form-group col-md-4">
                        <label for="txtFechaCreacion">Fecha creación de solicitud</label>
                        <input type="text" class="form-control" id="txtFechaCreacion" name="txtFechaCreacion" placeholder="Fecha creación" readonly="readonly" value="<%= ViewData["txtFechaCreacion"] %>" />
                    </fieldset>
                </div>
            </div>
        </div>

        <%--FORMULARIO--%>
        <div class="panel panel-primary">
            <div class="panel-heading">Datos solicitud</div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-12">
                        <label for="tablaRecursos">Recursos</label>
                        <table class="table table-bordered table-hover table-responsive" id="tablaRecursos">
                            <thead class="thead-default">
                                <tr>
                                    <th>Nombre</th>
                                    <th>Unidad</th>
                                    <th>Cantidad</th>
                                    <th>Eliminar</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr id="row1">
                                    <td>
                                        <input type="text" name="txtNombreRecurso1" id="txtNombreRecurso1" placeholder="Nombre" class="form-control" />
                                    </td>
                                    <td>
                                        <select class="form-control" name="txtUnidadRecurso1" id="txtUnidadRecurso1">
                                            <option value="0">-- Seleccione opción --</option>
                                            <%= ViewData["txtUnidadRecurso"] %>
                                        </select>
                                    </td>
                                    <td>
                                        <input type="text" name="txtCantidadRecurso1" id="txtCantidadRecurso1" placeholder="Cantidad" class="form-control" />
                                    </td>
                                    <td>
                                        <button type="button" id="btnEliminar" class="btn btn-danger center-block">
                                            <i class="icon-user glyphicon glyphicon-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td>
                                        <button type="button" id="btnAgregar" class="btn btn-success center-block">
                                            <i class="icon-user glyphicon glyphicon-plus"></i>
                                        </button>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>
                 <div class="row">
                    <fieldset class="form-group col-md-4">
                        <label for="txtDocumento">Documento</label>
                        <input type="file" class="form-control" id="txtDocumento" name="txtDocumento"/>
                    </fieldset>
                </div>
            </div>
        </div>

        <%--<div class="panel panel-primary">
            <div class="panel-heading">Datos solicitud</div>
            <div class="panel-body">
                <div class="row">
                    <fieldset class="form-group col-md-4">
                        <label for="txtNombres">
                            Nombres
                            <label style="color: red">*</label></label>
                        <input type="text" name="txtNombres" id="txtNombres" placeholder="Nombres" class="form-control" />
                    </fieldset>
                    <fieldset class="form-group col-md-4">
                        <label for="txtApellidoPaterno">
                            Apellido paterno
                            <label style="color: red">*</label></label>
                        <input type="text" name="txtApellidoPaterno" id="txtApellidoPaterno" placeholder="Apellido paterno" class="form-control" />
                    </fieldset>
                    <fieldset class="form-group col-md-4">
                        <label for="txtApellidoMaterno">
                            Apellido materno
                            <label style="color: red">*</label></label>
                        <input type="text" name="txtApellidoMaterno" id="txtApellidoMaterno" placeholder="Apellido materno" class="form-control" />
                    </fieldset>
                </div>
                <div class="row">
                    <fieldset class="form-group col-md-4">
                        <label for="txtFechaNacimiento">
                            Fecha de nacimiento
                            <label style="color: red">*</label></label>
                        <input type="date" name="txtFechaNacimiento" id="txtFechaNacimiento" class="form-control" />
                    </fieldset>
                    <fieldset class="form-group col-md-4">
                        <label for="txtNacionalidad">
                            Nacionalidad
                            <label style="color: red">*</label></label>
                        <select name="txtNacionalidad" id="txtNacionalidad" class="form-control">
                            <option value="0">-- Seleccione opción --</option>
                            <option value="1">Chilena</option>
                        </select>
                    </fieldset>
                </div>
            </div>
        </div>--%>

        <label style="color: red; font-size: 20px;">* </label>
        <label>Campos obligatorios</label>
        <input type="submit" name="btnEnviar" value="Crear solicitud" class="btn btn-primary btn-lg center-block text-center" />
    </form>

</asp:Content>
