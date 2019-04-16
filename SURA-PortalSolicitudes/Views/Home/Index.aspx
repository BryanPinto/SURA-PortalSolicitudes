<%@ Page Title="" Language="C#" MasterPageFile="~/Views/DisenoBootstrap3.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <script src="<%: Url.Content("~/Styles/js/moment.js") %>"></script>
    <script src="<%: Url.Content("~/Styles/js/datatable.min.js") %>"></script>
    <link href="<%: Url.Content("~/Styles/css/datatable.min.css") %>" rel="stylesheet" />

    <title>Estado de solicitudes</title>
    <script>
        $(document).ready(function () {
            // Datatable y propiedades
            var table = $('#tablasolicitudes').DataTable({
                "sDom": '<"top">rt<"bottom"ip><"clear">',
                "order": [[1, "desc"]],
                "language": {
                    "sProcessing": "Procesando...",
                    "sLengthMenu": "Mostrar _MENU_ registros",
                    "sZeroRecords": "No se encontraron resultados",
                    "sEmptyTable": "Ningún dato disponible en esta tabla",
                    "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
                    "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
                    "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
                    "sInfoPostFix": "",
                    "sSearch": "Buscar:",
                    "sUrl": "",
                    "sInfoThousands": ",",
                    "sLoadingRecords": "Cargando...",
                    "oPaginate": {
                        "sFirst": "Primero",
                        "sLast": "Último",
                        "sNext": "Siguiente",
                        "sPrevious": "Anterior"
                    },
                    "oAria": {
                        "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
                        "sSortDescending": ": Activar para ordenar la columna de manera descendente"
                    }
                }
            });

            $("#formFiltros").submit(function (e) {
                e.preventDefault();
                // Buscar cabeceras
                $.ajax({
                    url: '<%: Url.Content("~/Casos/BusquedaCasos/") %>',
                    data: $("#formFiltros").serialize(),
                    cache: false,
                    type: "POST",
                    success: function (data) {
                        console.log("data");
                        console.log(data);
                        if (data != "error") {
                            $('#tablasolicitudes').find('tbody').hide();
                            table.clear();
                            table.rows.add(JSON.parse(data));
                            table.draw();
                            $('#tablasolicitudes').find('tbody').fadeIn("slow");
                        }
                        else
                            alert("Error al buscar");
                    },
                    error: function () {
                        alert("Error al buscar");
                    }
                });
            });

            // Validar números
            $("#txtNroCaso").numeric("{ negative : false , decimalPlaces : 0 , decimal : ',' }");
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Estado de solicitudes</h2>

    <%--FILTROS--%>
    <form id="formFiltros">
        <div class="panel panel-primary">
            <div class="panel-heading">Filtros solicitudes</div>
            <div class="panel-body">
                <div class="row">
                    <fieldset class="form-group col-md-1 ">
                    </fieldset>
                    <fieldset class="form-group col-md-2 ">
                        <label for="txtFechaDesde">Fecha desde</label>
                        <input type="date" class="form-control" id="txtFechaDesde" name="txtFechaDesde" />
                    </fieldset>
                    <fieldset class="form-group col-md-2">
                        <label for="txtFechaHasta">Fecha hasta</label>
                        <input type="date" class="form-control" id="txtFechaHasta" name="txtFechaHasta" />
                    </fieldset>
                    <fieldset class="form-group col-md-2">
                        <label for="txtNroCaso">ID de caso</label>
                        <input type="text" class="form-control" id="txtNroCaso" name="txtNroCaso" placeholder="ID de caso" />
                    </fieldset>
                    <fieldset class="form-group col-md-2">
                        <label for="txtEstadoCaso">Estado del caso</label>
                        <select name="txtEstadoCaso" id="txtEstadoCaso" class="form-control">
                            <option value="0">Todos</option>
                            <option value="1">Iniciado</option>
                            <option value="2">Completado</option>
                        </select>
                    </fieldset>
                    <fieldset class="form-group col-md-2">
                        <label for="btnBuscarListar"></label>
                        <input type="submit" id="btnBuscarListar" value="Buscar" class="btn btn-primary form-control">
                    </fieldset>
                </div>
            </div>
        </div>
    </form>

    <%--TABLA--%>
    <div class="table-responsive">
        <table class="table table-bordered table-hover" id="tablasolicitudes">
            <thead>
                <tr>
                    <th class="col-xs-1">ID de caso</th>
                    <th class="col-xs-2">Estado del caso</th>
                    <th class="col-xs-1">Fecha de creación</th>
                    <%--<th class="col-xs-1">Fecha de solución</th>--%>
                    <th class="col-xs-4">Actividad actual</th>
                    <th class="col-xs-1">Detalle</th>
                </tr>
            </thead>
            <tbody><%= ViewData["valores"] %></tbody>
        </table>
    </div>

</asp:Content>
