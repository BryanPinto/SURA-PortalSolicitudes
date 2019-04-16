<%@ Page Title="" Language="C#" MasterPageFile="~/Views/DisenoBootstrap3.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <title>Resumen de caso</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Resumen caso <%= ViewData["txtNroCaso"] %></h2>

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
                            </tr>
                        </thead>
                        <tbody>
                            <%= ViewData["txtTabla"] %>
                        </tbody>
                    </table>
                </div>
            </div>
            
            <% if (ViewData["txtDocumento"] != null) { %>
            <div class="row">
                <fieldset class="form-group col-md-4">
                    <label for="txtDocumento">Documento</label>
                    <br/>
                    <%= ViewData["txtDocumento"] %>
                </fieldset>
            </div>
            <% } %>
        </div>
    </div>


</asp:Content>
