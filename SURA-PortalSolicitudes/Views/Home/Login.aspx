<%@ Page Title="" Language="C#" MasterPageFile="~/Views/DisenoBootstrap3.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <title>Login</title>
    <link href="<%: Url.Content("~/Styles/css/custom.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="card card-container">
            <img id="profile-img" class="profile-img-card" src="<%: Url.Content("~/Styles/image/logo.png") %>" />
            <%--<p id="profile-name" class="profile-name-card">Inicie sesión con su usuario de Active Directory</p>--%>
            <form class="form-signin" method="post" action="<%: Url.Content("~/Home/Login") %>">
                <span id="reauth-email" class="reauth-email"></span>
                <input type="text" id="inputEmail" name="txtUsuario" class="form-control" placeholder="Usuario" required autofocus>
                <input type="password" id="inputPassword" name="txtPass" class="form-control" placeholder="Contraseña" required>
                <br />
                <fieldset class="has-error">
                    <span class="help-block"><%= ViewData["mensaje"] %></span>
                </fieldset>
                <button class="btn btn-md btn-primary btn-block btn-signin" type="submit">Iniciar sesión</button>
                <br />
            </form>
        </div>
        <!-- /card-container -->
    </div>
    <!-- /container -->
</asp:Content>
