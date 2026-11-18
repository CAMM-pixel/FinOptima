using System;
using System.Collections.Generic;
using System.Text;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Modelos;

namespace FinOptima.Bot
{
    public static class RespuestasBot
    {
        private const int MaximoAlertas = 10;

        public static string Ayuda()
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("FinOptima - asistente de sobres");
            texto.AppendLine();
            texto.AppendLine("Registra tus movimientos escribiendo en lenguaje natural.");
            texto.AppendLine();
            texto.AppendLine("Gastos:");
            texto.AppendLine("- Gaste 80000 en el cine");
            texto.AppendLine("- Pague 45000 de mercado");
            texto.AppendLine("- Gaste 50 mil en transporte");
            texto.AppendLine();
            texto.AppendLine("Ingresos:");
            texto.AppendLine("- Me llego un ingreso de 200000 por una venta");
            texto.AppendLine("- Me llego el sueldo de 3.500.000");
            texto.AppendLine();
            texto.AppendLine("El sueldo se reparte entre tus 7 sobres segun el porcentaje objetivo de cada uno.");
            texto.AppendLine();
            texto.AppendLine("Comandos:");
            texto.AppendLine("/saldo - Resumen del mes");
            texto.AppendLine("/sobres - Saldo de cada sobre");
            texto.AppendLine("/alertas - Alertas activas");
            texto.AppendLine("/ingreso <monto> <descripcion>");
            texto.AppendLine("/gasto <monto> <descripcion>");
            texto.AppendLine("/ayuda - Esta lista");
            return texto.ToString();
        }

        public static string AyudaCorta()
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("No entendi tu mensaje. Prueba con:");
            texto.AppendLine("- Gaste 80000 en mercado");
            texto.AppendLine("- Me llego el sueldo de 3.000.000");
            texto.AppendLine();
            texto.AppendLine("Escribe /ayuda para ver todos los comandos.");
            return texto.ToString();
        }

        public static string Propuesta(OperacionPendiente operacion)
        {
            StringBuilder texto = new StringBuilder();
            string tipo = operacion.Tipo == TipoMovimiento.Ingreso ? "un INGRESO" : "un GASTO";

            texto.AppendLine("Voy a registrar " + tipo + ".");
            texto.AppendLine();
            texto.AppendLine("Monto: " + Moneda(Total(operacion)));
            texto.AppendLine("Categoria: " + operacion.Categoria);
            texto.AppendLine("Descripcion: " + operacion.Descripcion);

            if (operacion.Asignaciones.Count == 1)
            {
                AsignacionSobre unica = operacion.Asignaciones[0];
                texto.AppendLine("Sobre: " + unica.SobreNombre + " (" + Moneda(unica.Monto) + ")");
            }
            else if (operacion.Asignaciones.Count > 1)
            {
                texto.AppendLine();
                texto.AppendLine("Reparto por sobres:");
                foreach (AsignacionSobre asignacion in operacion.Asignaciones)
                {
                    texto.AppendLine("- " + asignacion.SobreNombre + ": " + Moneda(asignacion.Monto)
                        + " (" + asignacion.Porcentaje.ToString("0.#") + "%)");
                }
            }
            else
            {
                texto.AppendLine("Sobre: sin sobre (no afecta saldos)");
            }

            texto.AppendLine();
            texto.AppendLine("Registro la operacion?");
            return texto.ToString();
        }

        public static string Confirmada(List<AsignacionSobre> asignaciones)
        {
            StringBuilder texto = new StringBuilder();
            decimal total = 0;

            foreach (AsignacionSobre asignacion in asignaciones)
            {
                total = total + asignacion.Monto;
            }

            texto.AppendLine("Registro completado.");
            texto.AppendLine();
            texto.AppendLine("Guarde " + asignaciones.Count + " movimiento(s) por " + Moneda(total) + ".");

            if (asignaciones.Count > 1)
            {
                texto.AppendLine();
                foreach (AsignacionSobre asignacion in asignaciones)
                {
                    texto.AppendLine("- " + asignacion.SobreNombre + ": " + Moneda(asignacion.Monto));
                }
            }

            return texto.ToString();
        }

        public static string Cancelada()
        {
            return "Registro cancelado. No se guardo ningun movimiento.";
        }

        public static string Expirada()
        {
            return "La confirmacion expiro. Envia el mensaje de nuevo si deseas registrarlo.";
        }

        public static string Resumen(ResumenDashboard resumen)
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("Resumen de " + resumen.NombreMes + " " + resumen.Anio);
            texto.AppendLine();
            texto.AppendLine("Ingresos del mes: " + Moneda(resumen.IngresosMes));
            texto.AppendLine("Gastos del mes: " + Moneda(resumen.GastosMes));
            texto.AppendLine("Disponible: " + Moneda(resumen.DisponibleMes));
            texto.AppendLine();
            texto.AppendLine("Saldo en sobres: " + Moneda(resumen.SaldoSobres));
            texto.AppendLine("Ahorro en metas: " + Moneda(resumen.AhorroMetas));
            texto.AppendLine("Deudas pendientes: " + Moneda(resumen.SaldoDeudas));
            texto.AppendLine();
            texto.AppendLine("Alertas activas: " + resumen.Alertas.Count);
            return texto.ToString();
        }

        public static string ListaSobres(List<Sobre> sobres)
        {
            StringBuilder texto = new StringBuilder();
            decimal total = 0;
            int activos = 0;

            foreach (Sobre sobre in sobres)
            {
                if (sobre.Activo)
                {
                    activos = activos + 1;
                }
            }

            if (activos == 0)
            {
                return "No tienes sobres activos.";
            }

            texto.AppendLine("Saldos de tus sobres");
            texto.AppendLine();

            foreach (Sobre sobre in sobres)
            {
                if (!sobre.Activo)
                {
                    continue;
                }

                total = total + sobre.Saldo;
                texto.AppendLine("- " + sobre.Nombre + ": " + Moneda(sobre.Saldo)
                    + " (objetivo " + sobre.PorcentajeObjetivo.ToString("0.#") + "%)");
            }

            texto.AppendLine();
            texto.AppendLine("Total: " + Moneda(total));
            return texto.ToString();
        }

        public static string ListaAlertas(List<Alerta> alertas)
        {
            if (alertas.Count == 0)
            {
                return "No hay alertas activas. Todo en orden.";
            }

            StringBuilder texto = new StringBuilder();
            int mostradas = 0;

            texto.AppendLine("Alertas activas (" + alertas.Count + ")");
            texto.AppendLine();

            foreach (Alerta alerta in alertas)
            {
                if (mostradas >= MaximoAlertas)
                {
                    break;
                }

                mostradas = mostradas + 1;
                texto.AppendLine(mostradas + ". [" + alerta.Nivel + "] " + alerta.Titulo);
                texto.AppendLine(alerta.Mensaje);
                texto.AppendLine();
            }

            if (alertas.Count > mostradas)
            {
                texto.AppendLine("Y " + (alertas.Count - mostradas) + " alerta(s) mas en la aplicacion.");
            }

            return texto.ToString();
        }

        public static string Moneda(decimal monto)
        {
            return monto.ToString("C0");
        }

        private static decimal Total(OperacionPendiente operacion)
        {
            decimal total = 0;

            foreach (AsignacionSobre asignacion in operacion.Asignaciones)
            {
                total = total + asignacion.Monto;
            }

            return total;
        }
    }
}
