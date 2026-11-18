using System;
using System.Collections.Generic;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;

namespace FinOptima.Bot
{
    public class ManejadorBot
    {
        private const int MinutosVigencia = 10;

        private ClienteTelegram cliente;
        private FabricaServicios servicios;
        private long chatAutorizado;
        private Dictionary<long, OperacionPendiente> pendientes;

        public ManejadorBot(ClienteTelegram clienteTelegram, FabricaServicios fabrica, long chatPermitido)
        {
            cliente = clienteTelegram;
            servicios = fabrica;
            chatAutorizado = chatPermitido;
            pendientes = new Dictionary<long, OperacionPendiente>();
        }

        public void Procesar(ActualizacionTelegram actualizacion)
        {
            if (actualizacion.ChatId != chatAutorizado)
            {
                if (actualizacion.EsCallback)
                {
                    cliente.ResponderCallback(actualizacion.CallbackId, "Chat no autorizado.");
                }
                else
                {
                    cliente.EnviarMensaje(actualizacion.ChatId, "Este bot es privado de FinOptima.");
                }

                return;
            }

            if (actualizacion.EsCallback)
            {
                ProcesarCallback(actualizacion);
            }
            else
            {
                ProcesarTexto(actualizacion);
            }
        }

        private void ProcesarTexto(ActualizacionTelegram actualizacion)
        {
            Interpretacion interpretacion = InterpretadorMensaje.Interpretar(actualizacion.Texto);

            switch (interpretacion.Tipo)
            {
                case TipoInterpretacion.Ayuda:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.Ayuda());
                    break;
                case TipoInterpretacion.Saldo:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.Resumen(servicios.Dashboard.Obtener()));
                    break;
                case TipoInterpretacion.Sobres:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.ListaSobres(servicios.Sobres.Listar()));
                    break;
                case TipoInterpretacion.Alertas:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.ListaAlertas(servicios.Alertas.Listar()));
                    break;
                case TipoInterpretacion.Ingreso:
                case TipoInterpretacion.Gasto:
                    Proponer(actualizacion, interpretacion);
                    break;
                default:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.AyudaCorta());
                    break;
            }
        }

        private void Proponer(ActualizacionTelegram actualizacion, Interpretacion interpretacion)
        {
            if (interpretacion.Monto <= 0)
            {
                cliente.EnviarMensaje(actualizacion.ChatId,
                    "No encontre un monto en tu mensaje. Ejemplo: \"Gaste 80000 en mercado\".");
                return;
            }

            OperacionPendiente operacion = new OperacionPendiente();
            operacion.Tipo = interpretacion.Tipo == TipoInterpretacion.Ingreso ? TipoMovimiento.Ingreso : TipoMovimiento.Gasto;
            operacion.Categoria = interpretacion.Categoria;
            operacion.Descripcion = interpretacion.Descripcion;

            if (operacion.Tipo == TipoMovimiento.Ingreso && interpretacion.EsSueldo)
            {
                operacion.Asignaciones = ConstruirDistribucion(interpretacion.Monto);
            }

            if (operacion.Asignaciones.Count == 0)
            {
                operacion.Asignaciones = ConstruirAsignacionUnica(interpretacion);
            }

            pendientes[actualizacion.ChatId] = operacion;

            cliente.EnviarMensajeConBotones(actualizacion.ChatId, RespuestasBot.Propuesta(operacion), "confirmar", "cancelar");
        }

        private List<AsignacionSobre> ConstruirDistribucion(decimal monto)
        {
            List<Sobre> activos = new List<Sobre>();
            decimal totalPorcentaje = 0;
            List<AsignacionSobre> asignaciones = new List<AsignacionSobre>();

            foreach (Sobre sobre in servicios.Sobres.Listar())
            {
                if (sobre.Activo && sobre.PorcentajeObjetivo > 0)
                {
                    activos.Add(sobre);
                    totalPorcentaje = totalPorcentaje + sobre.PorcentajeObjetivo;
                }
            }

            if (activos.Count == 0 || totalPorcentaje <= 0)
            {
                return asignaciones;
            }

            decimal acumulado = 0;

            for (int posicion = 0; posicion < activos.Count; posicion = posicion + 1)
            {
                Sobre sobre = activos[posicion];
                decimal parte;

                if (posicion == activos.Count - 1)
                {
                    parte = monto - acumulado;
                }
                else
                {
                    parte = decimal.Round(monto * sobre.PorcentajeObjetivo / totalPorcentaje, 2, MidpointRounding.AwayFromZero);
                    acumulado = acumulado + parte;
                }

                if (parte <= 0)
                {
                    continue;
                }

                asignaciones.Add(CrearAsignacion(sobre, parte, sobre.PorcentajeObjetivo));
            }

            return asignaciones;
        }

        private List<AsignacionSobre> ConstruirAsignacionUnica(Interpretacion interpretacion)
        {
            List<AsignacionSobre> asignaciones = new List<AsignacionSobre>();
            Sobre sobre = ElegirSobre(servicios.Sobres.Listar(), interpretacion.SobreSugerido, interpretacion.Descripcion);

            AsignacionSobre asignacion = new AsignacionSobre();
            asignacion.Monto = interpretacion.Monto;

            if (sobre != null)
            {
                asignacion.SobreId = sobre.Id;
                asignacion.SobreNombre = sobre.Nombre;
                asignacion.Porcentaje = sobre.PorcentajeObjetivo;
            }

            asignaciones.Add(asignacion);
            return asignaciones;
        }

        private Sobre ElegirSobre(List<Sobre> sobres, TipoSobre tipo, string descripcion)
        {
            string texto = InterpretadorMensaje.Normalizar(descripcion);

            foreach (Sobre sobre in sobres)
            {
                if (!sobre.Activo || sobre.Nombre.Length == 0)
                {
                    continue;
                }

                string nombre = InterpretadorMensaje.Normalizar(sobre.Nombre);

                if (InterpretadorMensaje.ContieneFrase(texto, nombre))
                {
                    return sobre;
                }
            }

            foreach (Sobre sobre in sobres)
            {
                if (sobre.Activo && sobre.Tipo == tipo)
                {
                    return sobre;
                }
            }

            foreach (Sobre sobre in sobres)
            {
                if (sobre.Activo)
                {
                    return sobre;
                }
            }

            return null;
        }

        private void ProcesarCallback(ActualizacionTelegram actualizacion)
        {
            string dato = actualizacion.DatosCallback == null ? "" : actualizacion.DatosCallback;

            if (dato == "cancelar")
            {
                pendientes.Remove(actualizacion.ChatId);
                cliente.ResponderCallback(actualizacion.CallbackId, "Cancelado");
                CerrarMensaje(actualizacion, RespuestasBot.Cancelada());
                return;
            }

            if (dato != "confirmar")
            {
                cliente.ResponderCallback(actualizacion.CallbackId, "Opcion no reconocida");
                return;
            }

            OperacionPendiente operacion;

            if (!pendientes.TryGetValue(actualizacion.ChatId, out operacion))
            {
                cliente.ResponderCallback(actualizacion.CallbackId, "La operacion expiro");
                CerrarMensaje(actualizacion, RespuestasBot.Expirada());
                return;
            }

            if (Vencida(operacion))
            {
                pendientes.Remove(actualizacion.ChatId);
                cliente.ResponderCallback(actualizacion.CallbackId, "La operacion expiro");
                CerrarMensaje(actualizacion, RespuestasBot.Expirada());
                return;
            }

            pendientes.Remove(actualizacion.ChatId);
            cliente.ResponderCallback(actualizacion.CallbackId, "Registrado");

            List<AsignacionSobre> guardadas = new List<AsignacionSobre>();
            string errores = "";

            foreach (AsignacionSobre asignacion in operacion.Asignaciones)
            {
                Resultado resultado = GuardarMovimiento(operacion, asignacion);

                if (resultado.Exito)
                {
                    guardadas.Add(asignacion);
                }
                else
                {
                    errores = errores + "- " + asignacion.SobreNombre + ": " + resultado.Mensaje + "\n";
                }
            }

            if (guardadas.Count == 0)
            {
                CerrarMensaje(actualizacion, "No se pudo registrar el movimiento.\n" + errores);
                return;
            }

            string texto = RespuestasBot.Confirmada(guardadas);

            if (errores.Length > 0)
            {
                texto = texto + "\nNo se pudieron guardar:\n" + errores;
            }

            CerrarMensaje(actualizacion, texto);
        }

        private Resultado GuardarMovimiento(OperacionPendiente operacion, AsignacionSobre asignacion)
        {
            Movimiento movimiento = new Movimiento();
            movimiento.Tipo = operacion.Tipo;
            movimiento.Categoria = operacion.Categoria;
            movimiento.Descripcion = operacion.Descripcion;
            movimiento.Monto = asignacion.Monto;
            movimiento.SobreId = asignacion.SobreId;
            movimiento.Fecha = DateTime.Today;

            return servicios.Movimientos.Guardar(movimiento);
        }

        private void CerrarMensaje(ActualizacionTelegram actualizacion, string texto)
        {
            try
            {
                cliente.EditarMensaje(actualizacion.ChatId, actualizacion.MensajeId, texto);
            }
            catch (Exception)
            {
                cliente.EnviarMensaje(actualizacion.ChatId, texto);
            }
        }

        private static bool Vencida(OperacionPendiente operacion)
        {
            TimeSpan transcurrido = DateTime.Now - operacion.Creada;
            return transcurrido.TotalMinutes > MinutosVigencia;
        }

        private static AsignacionSobre CrearAsignacion(Sobre sobre, decimal monto, decimal porcentaje)
        {
            AsignacionSobre asignacion = new AsignacionSobre();
            asignacion.SobreId = sobre.Id;
            asignacion.SobreNombre = sobre.Nombre;
            asignacion.Monto = monto;
            asignacion.Porcentaje = porcentaje;
            return asignacion;
        }
    }
}
