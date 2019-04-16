using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebSolicitudes.Models
{
    public class Actividad
    {
        private int idCase;
        private string nombreActividad;

        public int IdCase { get => idCase; set => idCase = value; }

        public string NombreActividad { get => nombreActividad; set => nombreActividad = value; }
    }
}
