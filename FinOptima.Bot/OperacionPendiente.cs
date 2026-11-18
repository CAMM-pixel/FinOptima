using System;
using System.Collections.Generic;
using FinOptima.Entidades.Enums;

namespace FinOptima.Bot
{
    public class OperacionPendiente
    {
        public TipoMovimiento Tipo { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public DateTime Creada { get; set; }
        public List<AsignacionSobre> Asignaciones { get; set; }

        public OperacionPendiente()
        {
            Tipo = TipoMovimiento.Gasto;
            Categoria = "";
            Descripcion = "";
            Creada = DateTime.Now;
            Asignaciones = new List<AsignacionSobre>();
        }
    }
}
