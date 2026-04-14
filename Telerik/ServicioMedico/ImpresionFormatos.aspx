<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImpresionFormatos.aspx.cs" Inherits="Telerik.ServicioMedico.ImpresionFormatos" %>
<% if (TipoDoc == "PASE" || TipoDoc == "EXAMEN") { %>
    <%= PaseHtml %>
<% } else { %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <title>Impresi&#243;n de Documento M&#233;dico</title>
    <style>
        body { font-family: Arial, sans-serif; background: #eee; margin: 0; padding: 0; color: #333; }
        .print-container { background: white; width: 210mm; min-height: 297mm; margin: 20px auto; padding: 20px; box-sizing: border-box; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        @media print {
            body { background: white; }
            .print-container { margin: 0; box-shadow: none; width: 100%; }
        }
        .header { display: flex; align-items: center; border-bottom: 2px solid #2c3e50; padding-bottom: 10px; margin-bottom: 20px; }
        .logo { width: 80px; height: 80px; background: #2c3e50; color: white; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 2rem; margin-right: 20px; border-radius: 8px; }
        .company-info { flex: 1; }
        .company-name { font-size: 1.5rem; font-weight: bold; color: #2c3e50; }
        .doc-title { text-align: right; }
        .doc-title h1 { margin: 0; font-size: 1.2rem; color: #c0392b; text-transform: uppercase; }

        .section { margin-bottom: 20px; }
        .section-title { background: #f2f4f7; padding: 5px 10px; font-weight: bold; font-size: 0.9rem; border-left: 4px solid #2c3e50; margin-bottom: 10px; text-transform: uppercase; }
        .grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; font-size: 0.85rem; }
        .field { margin-bottom: 8px; }
        .field label { display: block; font-size: 0.7rem; color: #7f8c8d; text-transform: uppercase; margin-bottom: 2px; }
        .field span { display: block; border-bottom: 1px solid #ddd; padding: 2px 0; font-weight: 500; min-height: 1.2rem; }
        .footer { margin-top: 50px; text-align: center; font-size: 0.8rem; color: #95a5a6; }
    </style>
</head>
<body>
    <div class="print-container">
        <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
            <div style="padding: 100px; text-align: center; color: #c0392b;">
                <h2>Error</h2>
                <p><%= ErrorMessage %></p>
            </div>
        <% } else if (Paciente != null) { %>
            <div class="header">
                <div class="logo">GCI</div>
                <div class="company-info">
                    <div class="company-name">Servicio M&#233;dico</div>
                </div>
                <div class="doc-title">
                    <h1>REPORTE M&#201;DICO</h1>
                    <p>Folio: <%= IdOrden.ToString("D6") %></p>
                </div>
            </div>

            <div class="section">
                <div class="section-title">Datos del Paciente</div>
                <div class="grid">
                    <div class="field"><label>Nombre</label><span><%= Paciente.NombreCompleto %></span></div>
                    <div class="field"><label>Edad</label><span><%= Paciente.Edad %></span></div>
                    <div class="field"><label>Puesto</label><span><%= Paciente.Puesto %></span></div>
                </div>
            </div>
            <!-- Otros campos genéricos si no es Examen oficial -->
        <% } %>
        <div class="footer">Sistema GCI - Servicio M&#233;dico</div>
    </div>
</body>
</html>
<% } %>
