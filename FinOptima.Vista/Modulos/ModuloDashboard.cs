using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Controles;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloDashboard : ModuloBase
    {
        private TarjetaPanel tarjetaCuenta;
        private Label etiquetaCuentaRotulo;
        private Label etiquetaCuentaSaldo;
        private Label etiquetaCuentaNota;
        private Label etiquetaMes;
        private Button botonActualizarCuenta;

        private TarjetaPanel tarjetaSaldo;
        private TarjetaPanel tarjetaFlujo;
        private TarjetaPanel tarjetaGastos;
        private MedidorSemicirculo medidorSaldo;
        private MedidorSemicirculo medidorFlujo;
        private MedidorSemicirculo medidorGastos;
        private Label valorSaldo;
        private Label valorFlujo;
        private Label valorGastos;

        private TarjetaPanel tarjetaTendencia;
        private GraficoLinea graficoTendencia;
        private TarjetaPanel tarjetaEstructura;
        private GraficoDona graficoEstructura;

        private TarjetaPanel tarjetaSobres;
        private DataGridView grillaSobres;
        private Label etiquetaTotalSobres;
        private TarjetaPanel tarjetaAlertas;
        private DataGridView grillaAlertas;
        private Label etiquetaSinAlertas;

        public ModuloDashboard(FabricaServicios servicios) : base(servicios, "Panel general")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;

            tarjetaCuenta = new TarjetaPanel();
            tarjetaCuenta.ColorFondo = tema.Superficie;
            tarjetaCuenta.ColorInicio = tema.Superficie;
            tarjetaCuenta.ColorFin = Tema.Mezclar(tema.Superficie, Acento, 34);
            tarjetaCuenta.ColorBorde = Tema.Mezclar(tema.Superficie, Acento, 42);

            etiquetaCuentaRotulo = CrearRotulo("CUENTA PRINCIPAL", 24, 18, 8.5f, true);
            tarjetaCuenta.Controls.Add(etiquetaCuentaRotulo);

            etiquetaCuentaSaldo = new Label();
            etiquetaCuentaSaldo.Text = "";
            etiquetaCuentaSaldo.Location = new Point(24, 40);
            etiquetaCuentaSaldo.Size = new Size(460, 40);
            etiquetaCuentaSaldo.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            etiquetaCuentaSaldo.ForeColor = tema.Texto;
            etiquetaCuentaSaldo.BackColor = Color.Transparent;
            etiquetaCuentaSaldo.AutoEllipsis = true;
            tarjetaCuenta.Controls.Add(etiquetaCuentaSaldo);

            etiquetaCuentaNota = new Label();
            etiquetaCuentaNota.Text = "";
            etiquetaCuentaNota.Location = new Point(26, 80);
            etiquetaCuentaNota.Size = new Size(620, 20);
            etiquetaCuentaNota.Font = new Font("Segoe UI", 9);
            etiquetaCuentaNota.ForeColor = tema.TextoSecundario;
            etiquetaCuentaNota.BackColor = Color.Transparent;
            tarjetaCuenta.Controls.Add(etiquetaCuentaNota);

            etiquetaMes = new Label();
            etiquetaMes.Text = "";
            etiquetaMes.AutoSize = false;
            etiquetaMes.TextAlign = ContentAlignment.MiddleCenter;
            etiquetaMes.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            etiquetaMes.ForeColor = tema.Texto;
            etiquetaMes.BackColor = Tema.Mezclar(tema.Superficie, Acento, 20);
            etiquetaMes.Size = new Size(150, 30);
            UtilidadesVista.Redondear(etiquetaMes, 15);
            tarjetaCuenta.Controls.Add(etiquetaMes);

            botonActualizarCuenta = UtilidadesVista.CrearBotonSecundario("Actualizar", 0, 0, 120, 32);
            botonActualizarCuenta.Click += AlActualizar;
            tarjetaCuenta.Controls.Add(botonActualizarCuenta);

            Controls.Add(tarjetaCuenta);

            tarjetaSaldo = CrearTarjetaMedidor("SALDO", "En sobres sobre ingresos", tema.Exito, out valorSaldo, out medidorSaldo);
            tarjetaFlujo = CrearTarjetaMedidor("FLUJO DE CAJA", "Disponible sobre ingresos", tema.Exito, out valorFlujo, out medidorFlujo);
            tarjetaGastos = CrearTarjetaMedidor("GASTOS", "Gasto sobre ingresos", tema.Peligro, out valorGastos, out medidorGastos);
            Controls.Add(tarjetaSaldo);
            Controls.Add(tarjetaFlujo);
            Controls.Add(tarjetaGastos);

            tarjetaTendencia = new TarjetaPanel();
            tarjetaTendencia.ColorFondo = tema.Superficie;
            Label rotuloTendencia = CrearRotulo("Tendencia de saldo", 22, 18, 12, true);
            rotuloTendencia.ForeColor = tema.Texto;
            tarjetaTendencia.Controls.Add(rotuloTendencia);
            tarjetaTendencia.Controls.Add(CrearRotulo("Ahorro acumulado de los ultimos 6 meses", 22, 42, 8.5f, false));

            graficoTendencia = new GraficoLinea();
            graficoTendencia.ColorLinea = Acento;
            graficoTendencia.ColorGrilla = tema.BordeSuave;
            graficoTendencia.ColorTexto = tema.TextoSecundario;
            tarjetaTendencia.Controls.Add(graficoTendencia);
            Controls.Add(tarjetaTendencia);

            tarjetaEstructura = new TarjetaPanel();
            tarjetaEstructura.ColorFondo = tema.Superficie;
            Label rotuloEstructura = CrearRotulo("Estructura de los gastos", 22, 18, 12, true);
            rotuloEstructura.ForeColor = tema.Texto;
            tarjetaEstructura.Controls.Add(rotuloEstructura);
            tarjetaEstructura.Controls.Add(CrearRotulo("Reparto por categoria del mes", 22, 42, 8.5f, false));

            graficoEstructura = new GraficoDona();
            graficoEstructura.ColorPista = tema.SuperficieAlterna;
            graficoEstructura.ColorTexto = tema.TextoSecundario;
            graficoEstructura.TextoVacio = "Sin gastos registrados este mes";
            tarjetaEstructura.Controls.Add(graficoEstructura);
            Controls.Add(tarjetaEstructura);

            tarjetaSobres = new TarjetaPanel();
            tarjetaSobres.ColorFondo = tema.Superficie;
            Label rotuloSobres = CrearRotulo("Distribucion de sobres", 22, 18, 12, true);
            rotuloSobres.ForeColor = tema.Texto;
            tarjetaSobres.Controls.Add(rotuloSobres);

            etiquetaTotalSobres = CrearRotulo("", 22, 42, 8.5f, false);
            tarjetaSobres.Controls.Add(etiquetaTotalSobres);

            grillaSobres = new GrillaDatos();
            UtilidadesVista.PrepararGrilla(grillaSobres);
            grillaSobres.MinimumSize = new Size(0, 0);
            grillaSobres.Columns.Add("Nombre", "Sobre");
            grillaSobres.Columns.Add("Objetivo", "Objetivo %");
            grillaSobres.Columns.Add("Saldo", "Saldo");
            grillaSobres.Columns.Add("Sugerido", "Monto sugerido");
            grillaSobres.Columns.Add("Estado", "Estado");
            tarjetaSobres.Controls.Add(grillaSobres);
            Controls.Add(tarjetaSobres);

            tarjetaAlertas = new TarjetaPanel();
            tarjetaAlertas.ColorFondo = tema.Superficie;
            Label rotuloAlertas = CrearRotulo("Alertas activas", 22, 18, 12, true);
            rotuloAlertas.ForeColor = tema.Texto;
            tarjetaAlertas.Controls.Add(rotuloAlertas);
            tarjetaAlertas.Controls.Add(CrearRotulo("Avisos de presupuesto, deudas, metas y sobres", 22, 42, 8.5f, false));

            grillaAlertas = new GrillaDatos();
            UtilidadesVista.PrepararGrilla(grillaAlertas);
            grillaAlertas.MinimumSize = new Size(0, 0);
            grillaAlertas.Columns.Add("Nivel", "Nivel");
            grillaAlertas.Columns.Add("Tipo", "Tipo");
            grillaAlertas.Columns.Add("Titulo", "Titulo");
            grillaAlertas.Columns.Add("Mensaje", "Mensaje");
            tarjetaAlertas.Controls.Add(grillaAlertas);

            etiquetaSinAlertas = new Label();
            etiquetaSinAlertas.Text = "No hay alertas activas. Todo va en orden.";
            etiquetaSinAlertas.TextAlign = ContentAlignment.MiddleCenter;
            etiquetaSinAlertas.Font = new Font("Segoe UI", 10);
            etiquetaSinAlertas.ForeColor = tema.TextoSecundario;
            etiquetaSinAlertas.BackColor = Color.Transparent;
            etiquetaSinAlertas.Visible = false;
            tarjetaAlertas.Controls.Add(etiquetaSinAlertas);
            etiquetaSinAlertas.BringToFront();
            Controls.Add(tarjetaAlertas);
        }

        private TarjetaPanel CrearTarjetaMedidor(string titulo, string nota, Color color, out Label valor, out MedidorSemicirculo medidor)
        {
            Tema tema = TemaActual.Actual;

            TarjetaPanel tarjeta = new TarjetaPanel();
            tarjeta.ColorFondo = tema.Superficie;

            Label rotulo = CrearRotulo(titulo, 20, 16, 8.5f, true);
            tarjeta.Controls.Add(rotulo);
            tarjeta.Controls.Add(CrearRotulo(nota, 20, 35, 8, false));

            medidor = new MedidorSemicirculo();
            medidor.ColorValor = color;
            medidor.ColorPista = tema.SuperficieAlterna;
            tarjeta.Controls.Add(medidor);

            valor = new Label();
            valor.Text = "";
            valor.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            valor.ForeColor = tema.Texto;
            valor.BackColor = Color.Transparent;
            valor.AutoEllipsis = true;
            valor.TextAlign = ContentAlignment.MiddleLeft;
            tarjeta.Controls.Add(valor);

            return tarjeta;
        }

        private Label CrearRotulo(string texto, int x, int y, float tamano, bool negrita)
        {
            Label rotulo = new Label();
            rotulo.Text = texto;
            rotulo.Location = new Point(x, y);
            rotulo.AutoSize = true;
            rotulo.Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular);
            rotulo.ForeColor = TemaActual.Actual.TextoSecundario;
            rotulo.BackColor = Color.Transparent;
            return rotulo;
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (tarjetaCuenta == null || Recalculando)
            {
                return;
            }

            Recalculando = true;
            SuspendLayout();

            try
            {
                int altoContenido = 58 + 112 + 16 + 200 + 16 + 300 + 16 + 280 + 16 + 220 + 8;
                int anchoCliente = ClientSize.Width;

                if (altoContenido > ClientSize.Height && !VerticalScroll.Visible)
                {
                    anchoCliente = anchoCliente - SystemInformation.VerticalScrollBarWidth;
                }

                int ancho = anchoCliente - Margen * 2;

                if (ancho < 1040)
                {
                    ancho = 1040;
                }

                tarjetaCuenta.SetBounds(Margen, 58, ancho, 112);
                etiquetaMes.Location = new Point(ancho - 24 - 150, 18);
                botonActualizarCuenta.Location = new Point(ancho - 24 - 150 - 12 - 120, 17);
                etiquetaCuentaSaldo.Width = AnchoSeguro(ancho - 500, 320);
                etiquetaCuentaNota.Width = AnchoSeguro(ancho - 500, 320);

                int yMedidores = 58 + 112 + 16;
                int altoMedidor = 200;
                int espacio = 16;
                int anchoMedidor = (ancho - espacio * 2) / 3;

                tarjetaSaldo.SetBounds(Margen, yMedidores, anchoMedidor, altoMedidor);
                tarjetaFlujo.SetBounds(Margen + anchoMedidor + espacio, yMedidores, anchoMedidor, altoMedidor);
                tarjetaGastos.SetBounds(Margen + (anchoMedidor + espacio) * 2, yMedidores, anchoMedidor, altoMedidor);

                int anchoInterior = anchoMedidor - 36;
                medidorSaldo.SetBounds(18, 48, anchoInterior, 112);
                medidorFlujo.SetBounds(18, 48, anchoInterior, 112);
                medidorGastos.SetBounds(18, 48, anchoInterior, 112);
                valorSaldo.SetBounds(20, 162, anchoInterior, 30);
                valorFlujo.SetBounds(20, 162, anchoInterior, 30);
                valorGastos.SetBounds(20, 162, anchoInterior, 30);

                int yGraficos = yMedidores + altoMedidor + 16;
                int altoGraficos = 300;
                int anchoTendencia = (ancho * 62) / 100;

                if (anchoTendencia < 620)
                {
                    anchoTendencia = 620;
                }

                int anchoEstructura = ancho - anchoTendencia - espacio;

                if (anchoEstructura < 340)
                {
                    anchoEstructura = 340;
                    anchoTendencia = ancho - anchoEstructura - espacio;
                }

                tarjetaTendencia.SetBounds(Margen, yGraficos, anchoTendencia, altoGraficos);
                tarjetaEstructura.SetBounds(Margen + anchoTendencia + espacio, yGraficos, anchoEstructura, altoGraficos);
                graficoTendencia.SetBounds(16, 66, anchoTendencia - 32, altoGraficos - 82);
                graficoEstructura.SetBounds(6, 58, anchoEstructura - 12, altoGraficos - 68);

                int ySobres = yGraficos + altoGraficos + 16;
                int altoSobres = 280;

                tarjetaSobres.SetBounds(Margen, ySobres, ancho, altoSobres);
                grillaSobres.SetBounds(16, 64, ancho - 32, altoSobres - 80);

                int yAlertas = ySobres + altoSobres + 16;
                int altoAlertas = 220;

                tarjetaAlertas.SetBounds(Margen, yAlertas, ancho, altoAlertas);
                grillaAlertas.SetBounds(16, 64, ancho - 32, altoAlertas - 80);
                etiquetaSinAlertas.SetBounds(16, 64, ancho - 32, altoAlertas - 80);

                AutoScrollMinSize = new Size(ancho + Margen * 2, altoContenido);
            }
            finally
            {
                ResumeLayout(false);
                Recalculando = false;
            }
        }

        private int AnchoSeguro(int valor, int minimo)
        {
            if (valor < minimo)
            {
                return minimo;
            }

            return valor;
        }

        private void CargarDatos()
        {
            try
            {
                ResumenDashboard resumen = Servicios.Dashboard.Obtener();
                etiquetaMes.Text = resumen.NombreMes + " " + resumen.Anio;
                etiquetaCuentaSaldo.Text = UtilidadesVista.Moneda(resumen.SaldoSobres);
                etiquetaCuentaNota.Text = "Saldo en sobres   ·   Disponible del mes: " + UtilidadesVista.Moneda(resumen.DisponibleMes);

                medidorSaldo.Valor = CalcularRatio(resumen.SaldoSobres, resumen.IngresosMes);
                medidorFlujo.Valor = CalcularRatio(resumen.DisponibleMes, resumen.IngresosMes);
                medidorGastos.Valor = CalcularRatio(resumen.GastosMes, resumen.IngresosMes);

                valorSaldo.Text = UtilidadesVista.Moneda(resumen.SaldoSobres);
                valorFlujo.Text = UtilidadesVista.Moneda(resumen.DisponibleMes);
                valorGastos.Text = UtilidadesVista.Moneda(resumen.GastosMes);

                medidorFlujo.ColorValor = resumen.DisponibleMes >= 0 ? TemaActual.Actual.Exito : TemaActual.Actual.Peligro;

                CargarSobres(resumen);
                CargarAlertas(resumen);
                CargarTendencia();
                CargarEstructura(resumen);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private double CalcularRatio(decimal parte, decimal total)
        {
            if (total <= 0)
            {
                return 0;
            }

            double ratio = (double)(parte / total);

            if (ratio < 0)
            {
                return 0;
            }

            if (ratio > 1)
            {
                return 1;
            }

            return ratio;
        }

        private void CargarSobres(ResumenDashboard resumen)
        {
            grillaSobres.Rows.Clear();
            decimal total = 0;

            foreach (SobreResumen sobre in resumen.Sobres)
            {
                total = total + sobre.Saldo;
                grillaSobres.Rows.Add(
                    sobre.Nombre,
                    UtilidadesVista.Porcentaje(sobre.PorcentajeObjetivo),
                    UtilidadesVista.Moneda(sobre.Saldo),
                    UtilidadesVista.Moneda(sobre.MontoSugerido),
                    sobre.Estado);
            }

            etiquetaTotalSobres.Text = "Total disponible: " + UtilidadesVista.Moneda(total);
        }

        private void CargarAlertas(ResumenDashboard resumen)
        {
            grillaAlertas.Rows.Clear();

            foreach (Alerta alerta in resumen.Alertas)
            {
                grillaAlertas.Rows.Add(alerta.Nivel, alerta.Tipo, alerta.Titulo, alerta.Mensaje);
            }

            bool hayAlertas = resumen.Alertas.Count > 0;
            grillaAlertas.Visible = hayAlertas;
            etiquetaSinAlertas.Visible = !hayAlertas;
        }

        private void CargarTendencia()
        {
            List<ResumenMensual> meses = Servicios.Analisis.ListarUltimosMeses(6);
            List<decimal> valores = new List<decimal>();
            List<string> etiquetas = new List<string>();
            decimal acumulado = 0;
            bool hayMovimientos = false;

            for (int indice = meses.Count - 1; indice >= 0; indice = indice - 1)
            {
                ResumenMensual mes = meses[indice];
                acumulado = acumulado + mes.Ahorro;
                valores.Add(acumulado);
                etiquetas.Add(mes.NombreMes);

                if (mes.Ingresos != 0 || mes.Gastos != 0)
                {
                    hayMovimientos = true;
                }
            }

            if (!hayMovimientos)
            {
                graficoTendencia.EstablecerDatos(new List<decimal>(), new List<string>());
                return;
            }

            graficoTendencia.EstablecerDatos(valores, etiquetas);
        }

        private void CargarEstructura(ResumenDashboard resumen)
        {
            ResumenMensual mensual = Servicios.Analisis.ObtenerResumen(resumen.Anio, resumen.Mes);
            Color[] colores = new Color[]
            {
                Color.FromArgb(23, 201, 100),
                Color.FromArgb(0, 145, 255),
                Color.FromArgb(245, 165, 36),
                Color.FromArgb(147, 97, 255),
                Color.FromArgb(243, 18, 96),
                Color.FromArgb(0, 184, 169)
            };

            List<SegmentoDona> segmentos = new List<SegmentoDona>();

            for (int indice = 0; indice < mensual.GastosPorCategoria.Count; indice = indice + 1)
            {
                DetalleCategoria detalle = mensual.GastosPorCategoria[indice];
                segmentos.Add(new SegmentoDona(detalle.Categoria, detalle.Monto, detalle.Porcentaje, colores[indice % colores.Length]));
            }

            graficoEstructura.EstablecerDatos(segmentos);
        }

        private void AlActualizar(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }
    }
}
