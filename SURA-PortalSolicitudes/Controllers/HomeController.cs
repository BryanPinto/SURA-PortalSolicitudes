using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml;

namespace WebSolicitudes.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Home/Login

        public ActionResult Login()
        {
            return View();
        }

        //
        // GET: /Home/Login
        [System.Web.Mvc.HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            return RedirectToAction("Index", "Home"); ;
        }
        

        //
        // GET: /Home/CerrarSesion

        public ActionResult CerrarSesion()
        {
            Session.Abandon();
            return RedirectToAction("Login", "Home"); ;
        }

        //
        // GET: /Home/Error404

        public ActionResult Error404()
        {
            return View();
        }

    }
}
