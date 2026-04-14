<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AntidopingPrint.aspx.cs" Inherits="Telerik.ServicioMedico.AntidopingPrint" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
<meta charset="utf-8" />
<title>Formato de Antidoping - GCI</title>
<style>
    @media print {
        body { margin: 0; padding: 15px; font-family: Arial, sans-serif; font-size: 12px; }
        .no-print { display: none !important; }
        .formato-container { width: 100%; max-width: 800px; margin: 0 auto; }
        table { width: 100%; border-collapse: collapse; margin-bottom: 15px; }
        .borde-completo { border: 2px solid #000; }
        .borde-inferior { border-bottom: 1px solid #000; }
        .borde-superior { border-top: 1px solid #000; }
        .celda-titulo { background-color: #f0f0f0; font-weight: bold; text-align: center; padding: 8px; border: 1px solid #000; }
        .celda-etiqueta { font-weight: bold; padding: 5px; border: 1px solid #000; background-color: #f9f9f9; width: 25%; }
        .celda-dato { padding: 5px; border: 1px solid #000; }
        .celda-centrada { text-align: center; padding: 5px; border: 1px solid #000; }
        .header-grande { font-size: 16px; font-weight: bold; text-align: center; padding: 10px; border: 2px solid #000; background-color: #e0e0e0; }
        .sub-header { font-size: 14px; font-weight: bold; text-align: center; padding: 6px; border: 1px solid #000; background-color: #f5f5f5; }
        .texto-informativo { text-align: justify; padding: 10px; border: 1px solid #000; line-height: 1.4; }
        .checkbox-container { display: flex; align-items: center; margin: 5px 0; }
        .checkbox { width: 15px; height: 15px; border: 1px solid #000; margin-right: 8px; }
        .firma-container { height: 60px; border-bottom: 1px solid #000; margin-top: 40px; }
        .firma-label { text-align: center; font-size: 11px; margin-top: 5px; }
        .pagina-break { page-break-after: always; }
    }
    
    @media screen {
        body { margin: 20px; font-family: Arial, sans-serif; background-color: #f5f5f5; }
        .formato-container { width: 100%; max-width: 800px; margin: 0 auto; background-color: white; padding: 20px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        table { width: 100%; border-collapse: collapse; margin-bottom: 15px; }
        .borde-completo { border: 2px solid #000; }
        .borde-inferior { border-bottom: 1px solid #000; }
        .borde-superior { border-top: 1px solid #000; }
        .celda-titulo { background-color: #f0f0f0; font-weight: bold; text-align: center; padding: 8px; border: 1px solid #000; }
        .celda-etiqueta { font-weight: bold; padding: 5px; border: 1px solid #000; background-color: #f9f9f9; width: 25%; }
        .celda-dato { padding: 5px; border: 1px solid #000; }
        .celda-centrada { text-align: center; padding: 5px; border: 1px solid #000; }
        .header-grande { font-size: 16px; font-weight: bold; text-align: center; padding: 10px; border: 2px solid #000; background-color: #e0e0e0; }
        .sub-header { font-size: 14px; font-weight: bold; text-align: center; padding: 6px; border: 1px solid #000; background-color: #f5f5f5; }
        .texto-informativo { text-align: justify; padding: 10px; border: 1px solid #000; line-height: 1.4; }
        .checkbox-container { display: flex; align-items: center; margin: 5px 0; }
        .checkbox { width: 15px; height: 15px; border: 1px solid #000; margin-right: 8px; }
        .firma-container { height: 60px; border-bottom: 1px solid #000; margin-top: 40px; }
        .firma-label { text-align: center; font-size: 11px; margin-top: 5px; }
        .pagina-break { page-break-after: always; }
    }
</style>
</head>
<body>
<form id="form1" runat="server">
<div class="formato-container">

    <!-- ENCABEZADO PRINCIPAL -->
    <table class="borde-completo">
        <tr>
            <td class="header-grande" colspan="4">
                GCI GRUPO CONSTRUCTOR INDUSTRIAL OIL & GAS<br/>
                FORMATO DE EXAMEN TOXICOLÓGICO (ANTIDOPING)
            </td>
        </tr>
        <tr>
            <td class="celda-etiqueta">PROYECTO:</td>
            <td class="celda-dato" id="proyecto">_________________________</td>
            <td class="celda-etiqueta">FECHA:</td>
            <td class="celda-dato" id="fecha">_________________________</td>
        </tr>
        <tr>
            <td class="celda-etiqueta">EMPRESA:</td>
            <td class="celda-dato" id="empresa">_________________________</td>
            <td class="celda-etiqueta">TURNO:</td>
            <td class="celda-dato" id="turno">_________________________</td>
        </tr>
    </table>

    <!-- TEXTO INFORMATIVO -->
    <table class="borde-completo">
        <tr>
            <td class="sub-header">POR ESTE CONDUCTO SE SOLICITA LA APLICACIÓN DE EXAMEN TOXICOLÓGICO AL SIGUIENTE TRABAJADOR:</td>
        </tr>
        <tr>
            <td class="texto-informativo">
                La empresa GCI GRUPO CONSTRUCTOR INDUSTRIAL OIL & GAS, en cumplimiento con las normas de seguridad y salud laboral, 
                solicita la aplicación de examen toxicológico para verificar que el trabajador no se encuentre bajo la influencia 
                de sustancias controladas que puedan afectar su desempeño y seguridad en el centro de trabajo.
            </td>
        </tr>
    </table>

    <!-- DATOS DEL TRABAJADOR -->
    <table class="borde-completo">
        <tr>
            <td class="celda-titulo" colspan="4">DATOS DEL TRABAJADOR</td>
        </tr>
        <tr>
            <td class="celda-etiqueta">NOMBRE COMPLETO:</td>
            <td class="celda-dato" colspan="3" id="nombreTrabajador">_______________________________________________________________</td>
        </tr>
        <tr>
            <td class="celda-etiqueta">PUESTO:</td>
            <td class="celda-dato" id="puesto">_________________________</td>
            <td class="celda-etiqueta">EDAD:</td>
            <td class="celda-dato" id="edad">_________________________</td>
        </tr>
        <tr>
            <td class="celda-etiqueta">NÚM. EMPLEADO:</td>
            <td class="celda-dato" id="numEmpleado">_________________________</td>
            <td class="celda-etiqueta">SEXO:</td>
            <td class="celda-dato" id="sexo">_________________________</td>
        </tr>
    </table>

    <!-- CONSENTIMIENTO INFORMADO -->
    <table class="borde-completo">
        <tr>
            <td class="sub-header">CONSENTIMIENTO INFORMADO</td>
        </tr>
        <tr>
            <td class="texto-informativo">
                Yo, _______________________________________________ manifiesto mi consentimiento voluntario para la realización 
                del examen toxicológico, entendiendo que los resultados serán confidenciales y utilizados únicamente para fines 
                laborales. Estoy consciente de que deberé proporcionar muestra (orina/sangre) en las condiciones que indique el 
                personal médico.
            </td>
        </tr>
        <tr>
            <td style="padding: 10px; border: 1px solid #000;">
                <div class="checkbox-container">
                    <div class="checkbox" id="chkConsentimiento"></div>
                    <span>ACEPTO LA REALIZACIÓN DEL EXAMEN TOXICOLÓGICO</span>
                </div>
            </td>
        </tr>
    </table>

    <!-- RESULTADOS DEL EXAMEN -->
    <table class="borde-completo">
        <tr>
            <td class="celda-titulo" colspan="6">RESULTADOS DEL EXAMEN TOXICOLÓGICO</td>
        </tr>
        <tr>
            <td class="celda-centrada" style="font-weight: bold;">SUSTANCIA</td>
            <td class="celda-centrada" style="font-weight: bold;">APLICA</td>
            <td class="celda-centrada" style="font-weight: bold;">RESULTADO</td>
            <td class="celda-centrada" style="font-weight: bold;">SUSTANCIA</td>
            <td class="celda-centrada" style="font-weight: bold;">APLICA</td>
            <td class="celda-centrada" style="font-weight: bold;">RESULTADO</td>
        </tr>
        <tr>
            <td class="celda-dato">Cocaína</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaCocaina"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoCocaina"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
            <td class="celda-dato">Anfetaminas</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaAnfetaminas"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoAnfetaminas"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
        </tr>
        <tr>
            <td class="celda-dato">THC (Marihuana)</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaTHC"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoTHC"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
            <td class="celda-dato">Metanfetaminas</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaMetanfetaminas"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoMetanfetaminas"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
        </tr>
        <tr>
            <td class="celda-dato">Alcohol</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaAlcohol"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoAlcohol"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
            <td class="celda-dato">Opiáceos</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaOpiaceos"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoOpiaceos"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
        </tr>
        <tr>
            <td class="celda-dato">Benzodiacepinas</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaBenzodiacepinas"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoBenzodiacepinas"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
            <td class="celda-dato">Otras</td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkAplicaOtras"></div>
                </div>
            </td>
            <td class="celda-centrada">
                <div class="checkbox-container" style="justify-content: center;">
                    <div class="checkbox" id="chkPositivoOtras"></div>
                    <span style="margin-left: 5px;">Positivo</span>
                </div>
            </td>
        </tr>
    </table>

    <!-- VEREDICTO FINAL -->
    <table class="borde-completo">
        <tr>
            <td class="celda-titulo">VEREDICTO FINAL</td>
        </tr>
        <tr>
            <td style="padding: 10px; border: 1px solid #000;">
                <div class="checkbox-container">
                    <div class="checkbox" id="chkVeredictoApto"></div>
                    <span>APTO - No se detectaron sustancias controladas</span>
                </div>
                <div class="checkbox-container">
                    <div class="checkbox" id="chkVeredictoNoApto"></div>
                    <span>NO APTO - Se detectaron sustancias controladas</span>
                </div>
            </td>
        </tr>
        <tr>
            <td class="celda-etiqueta">OBSERVACIONES/COMENTARIOS:</td>
        </tr>
        <tr>
            <td style="padding: 10px; border: 1px solid #000; height: 60px;" id="observaciones">
                ________________________________________________________________________________<br/>
                ________________________________________________________________________________<br/>
                ________________________________________________________________________________
            </td>
        </tr>
    </table>

    <!-- EVIDENCIA FOTOGRÁFICA -->
    <table class="borde-completo">
        <tr>
            <td class="celda-titulo">EVIDENCIA FOTOGRÁFICA</td>
        </tr>
        <tr>
            <td style="padding: 10px; border: 1px solid #000; text-align: center;">
                <div style="width: 150px; height: 150px; border: 2px dashed #000; margin: 0 auto; display: flex; align-items: center; justify-content: center;">
                    <span id="fotoContainer">[FOTO DEL TRABAJADOR]</span>
                </div>
            </td>
        </tr>
    </table>

    <!-- FIRMAS -->
    <table class="borde-completo">
        <tr>
            <td style="width: 33%; padding: 10px; border: 1px solid #000; text-align: center;">
                <div class="firma-container"></div>
                <div class="firma-label">FIRMA DEL TRABAJADOR</div>
            </td>
            <td style="width: 33%; padding: 10px; border: 1px solid #000; text-align: center;">
                <div class="firma-container"></div>
                <div class="firma-label">FIRMA DEL MÉDICO</div>
            </td>
            <td style="width: 33%; padding: 10px; border: 1px solid #000; text-align: center;">
                <div class="firma-container"></div>
                <div class="firma-label">FIRMA DEL REPRESENTANTE DE GCI</div>
            </td>
        </tr>
    </table>

    <!-- PIE DE PÁGINA -->
    <table class="borde-completo">
        <tr>
            <td style="padding: 8px; border: 1px solid #000; text-align: center; font-size: 10px;">
                GCI GRUPO CONSTRUCTOR INDUSTRIAL OIL & GAS | DEPARTAMENTO DE SERVICIO MÉDICO | FORMATO: ANT-001 V1.0
            </td>
        </tr>
    </table>

    <!-- BOTONES DE IMPRESIÓN (SOLO VISTA SCREEN) -->
    <div class="no-print" style="text-align: center; margin-top: 20px;">
        <button type="button" onclick="window.print()" style="padding: 10px 20px; font-size: 14px; background-color: #007bff; color: white; border: none; cursor: pointer; margin-right: 10px;">
            <i class="fas fa-print"></i> IMPRIMIR
        </button>
        <button type="button" onclick="window.close()" style="padding: 10px 20px; font-size: 14px; background-color: #6c757d; color: white; border: none; cursor: pointer;">
            <i class="fas fa-times"></i> CERRAR
        </button>
    </div>

</div>
</form>

<script>
    // Función para cargar datos del examen antidoping
    function cargarDatosAntidoping(pkOrden) {
        // Aquí se pueden cargar los datos via AJAX si se necesita
        // Por ahora, los datos se pueden llenar desde code-behind
    }

    // Función para marcar checkboxes
    function marcarCheckbox(id, marcado) {
        const checkbox = document.getElementById(id);
        if (checkbox) {
            if (marcado) {
                checkbox.innerHTML = '✔';
                checkbox.style.backgroundColor = '#000';
                checkbox.style.color = '#fff';
            } else {
                checkbox.innerHTML = '';
                checkbox.style.backgroundColor = '#fff';
                checkbox.style.color = '#000';
            }
        }
    }

    // Auto-imprimir al cargar si se pasa parámetro
    window.onload = function() {
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.get('autoPrint') === 'true') {
            setTimeout(() => {
                window.print();
            }, 500);
        }
    };
</script>

</body>
</html>
