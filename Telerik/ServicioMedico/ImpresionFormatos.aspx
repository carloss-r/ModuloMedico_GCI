<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImpresionFormatos.aspx.cs" Inherits="Telerik.ServicioMedico.ImpresionFormatos" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Impresión de Documento Médico</title>
    <style>
        body { font-family: Arial, sans-serif; background: #eee; margin: 0; padding: 0; color: #333; }
        .print-container { background: white; width: 210mm; min-height: 297mm; margin: 20px auto; padding: 20px; box-sizing: border-box; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        @media print {
            body { background: white; }
            .print-container { margin: 0; box-shadow: none; width: 100%; }
            .no-print { display: none; }
        }
        .header { display: flex; align-items: center; border-bottom: 2px solid #2c3e50; padding-bottom: 10px; margin-bottom: 20px; }
        .logo { width: 80px; height: 80px; background: #2c3e50; color: white; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 2rem; margin-right: 20px; border-radius: 8px; }
        .company-info { flex: 1; }
        .company-name { font-size: 1.5rem; font-weight: bold; color: #2c3e50; }
        .doc-title { text-align: right; }
        .doc-title h1 { margin: 0; font-size: 1.2rem; color: #c0392b; text-transform: uppercase; }
        .doc-title p { margin: 5px 0 0; font-size: 0.9rem; color: #7f8c8d; }

        .section { margin-bottom: 20px; }
        .section-title { background: #f2f4f7; padding: 5px 10px; font-weight: bold; font-size: 0.9rem; border-left: 4px solid #2c3e50; margin-bottom: 10px; text-transform: uppercase; }
        
        .grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; font-size: 0.85rem; }
        .field { margin-bottom: 8px; }
        .field label { display: block; font-size: 0.7rem; color: #7f8c8d; text-transform: uppercase; margin-bottom: 2px; }
        .field span { display: block; border-bottom: 1px solid #ddd; padding: 2px 0; font-weight: 500; min-height: 1.2rem; }

        .footer { margin-top: 50px; text-align: center; font-size: 0.8rem; color: #95a5a6; }
        .signature-area { display: flex; justify-content: space-around; margin-top: 60px; }
        .sig-box { width: 200px; border-top: 1px solid #333; text-align: center; padding-top: 5px; font-size: 0.85rem; }

        /* Estilos específicos para el PASE */
        .pase-card { border: 2px dashed #3498db; padding: 20px; border-radius: 10px; position: relative; }
        .pase-folio { position: absolute; top: 10px; right: 20px; font-size: 1.5rem; font-weight: bold; color: #3498db; }
    </style>
</head>
<body>

    <div class="print-container">
        <!-- HEADER SOLO SI NO ES PASE OFICIAL (El pase oficial y el EXAMEN oficial traen su propio header en el HTML) -->
        <% if (TipoDoc != "PASE" && TipoDoc != "EXAMEN" && string.IsNullOrEmpty(ErrorMessage)) { %>
            <div class="header">
                <div class="logo">GCI</div>
                <div class="company-info">
                    <div class="company-name">Servicio Médico & GCI</div>
                    <div style="font-size: 0.8rem; color: #7f8c8d;">Departamento de Recursos Humanos / Salud Ocupacional</div>
                </div>
                <div class="doc-title">
                    <h1>
                        <% if (TipoDoc == "EXAMEN") { %> Resultado de Evaluación Médica <% } %>
                        <% else if (TipoDoc == "AD") { %> Resultado de Antidoping <% } %>
                    </h1>
                    <p>Folio: <%= Paciente != null ? IdOrden.ToString("D6") : "000000" %></p>
                </div>
            </div>
        <% } %>

        <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
            <div style="padding: 100px; text-align: center; color: #c0392b; border: 2px solid #c0392b; border-radius: 10px;">
                <h2><i class="fas fa-exclamation-triangle"></i> Error en la Generación</h2>
                <p><%= ErrorMessage %></p>
                <button onclick="window.close()" style="margin-top:20px; padding:8px 15px; background:#c0392b; color:white; border:none; cursor:pointer;">Cerrar Ventana</button>
            </div>
        <% } else if (Paciente != null) { %>
            
            <% if (TipoDoc == "PASE" || TipoDoc == "EXAMEN") { %>
                <!-- RENDER DINAMICO (HTML OFICIAL) -->
                <%= PaseHtml %>
            <% } else { %>
                <!-- CONTENIDO DE RESULTADOS (EXAMEN O AD) -->
                <div class="section">
                    <div class="section-title">Información del Paciente</div>
                    <div class="grid" style="grid-template-columns: 2fr 1fr 1fr;">
                        <div class="field"><label>Nombre</label><span><%= Paciente.NombreCompleto %></span></div>
                        <div class="field"><label>Edad</label><span><%= Paciente.Edad %> años</span></div>
                        <div class="field"><label>Sexo</label><span><%= Paciente.Sexo %></span></div>
                    </div>
                    <div class="grid">
                        <div class="field"><label>Puesto</label><span><%= Paciente.Puesto %></span></div>
                        <div class="field"><label>Empresa</label><span><%= Paciente.Empresa %></span></div>
                        <div class="field"><label>Fecha de Examen</label><span><%= DateTime.Now.ToShortDateString() %></span></div>
                    </div>
                </div>

                <% if (Evaluacion != null) { %>
                <div class="section">
                    <div class="section-title">Signos Vitales y Somatometría</div>
                    <div class="grid" style="grid-template-columns: repeat(4, 1fr);">
                        <div class="field"><label>Presión Art.</label><span><%= Evaluacion.PresionSistolica %>/<%= Evaluacion.PresionDiastolica %> mmHg</span></div>
                        <div class="field"><label>Peso / Estatura</label><span><%= Evaluacion.PesoKg %> kg / <%= Evaluacion.AlturaMetros %> m</span></div>
                        <div class="field"><label>IMC</label><span><%= Evaluacion.Imc %> (<%= Evaluacion.ImcDescripcion %>)</span></div>
                        <div class="field"><label>Glucosa</label><span><%= Evaluacion.Glucosa %> mg/dl</span></div>
                    </div>
                </div>
                
                <div class="section">
                    <div class="section-title">Diagnóstico y Aptitud</div>
                    <div class="field" style="margin-bottom:15px;">
                        <label>Observaciones Médicas / Diagnóstico</label>
                        <div style="border-bottom: 1px solid #ddd; min-height: 60px; padding: 5px 0; font-size: 0.9rem;">
                            <%= Evaluacion.Observaciones %>
                        </div>
                    </div>
                    <div class="field">
                        <label>Veredicto / Aptitud</label>
                        <span style="font-size: 1.1rem; color: #2c3e50; font-weight: bold;"><%= Evaluacion.AptitudMedicaDesc ?? "PENDIENTE" %></span>
                    </div>
                </div>
                <% } %>

                <div class="signature-area">
                    <div class="sig-box">Firma del Paciente</div>
                    <div class="sig-box">Firma y Cédula del Médico</div>
                </div>
            <% } %>

        <% } %>

        <% if (TipoDoc != "PASE") { %>
        <div class="footer">
            Este documento es estrictamente confidencial.<br>
            Generado por el Módulo de RH - GCI Software.
        </div>
        <% } %>
    </div>
</body>
</html> 
