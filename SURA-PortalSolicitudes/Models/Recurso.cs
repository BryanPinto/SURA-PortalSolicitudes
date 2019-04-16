using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolicitudes.Models
{
    public class Recurso
    {
        private string nombre;
        private int unidad;
        private int cantidad;

        public Recurso()
        {
        }

        public Recurso(string nombre, int unidad, int cantidad)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Unidad = unidad;
            Cantidad = cantidad;
        }

        public string Nombre { get => nombre; set => nombre = value; }
        public int Unidad { get => unidad; set => unidad = value; }
        public int Cantidad { get => cantidad; set => cantidad = value; }
    }
}