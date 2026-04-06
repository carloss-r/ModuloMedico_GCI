// FormularioEvaluacionMedica.js
// Entry point and orchestration for the Medical Evaluation Form

var cambiosSinGuardar = false;

$(document).ready(function() {
    // Detect changes to warn before leaving
    $('input, select, textarea').on('change input', function() {
        cambiosSinGuardar = true;
    });

    window.onbeforeunload = null;

    // Initialize formatting and input masks
    initInputFormatters();

    // Initialize dynamic tables
    initForms();

    // Load initial patient data (idOrden is injected in the view)
    loadPatientData(idOrden);

    // Contextual UI behavior
    $('#txtFechaNacimiento').on('change', function() {
        calcularEdad();
    });

    $('#ddlEscolaridad').on('change', function() {
        var val = ($(this).val() || "").toUpperCase().trim();
        var show = (val === 'UNIVERSIDAD' || val === 'POSGRADO' || val === 'LICENCIATURA');
        if (!show) $('#txtProfesion').val('');
        $('#txtProfesion').prop('disabled', !show).prop('placeholder', show ? 'Especifique su carrera' : 'N/A');
    });

    // Antecedentes Checkboxes toggle detalles
    $('#tbAntecedentesHF, #tbAntecedentesPP').on('change', '.chk-ant', function() {
        var $det = $(this).closest('tr').find('.ant-det');
        if(this.checked) {
            $det.prop('disabled', false).removeClass('blocked');
        } else {
            $det.val('').prop('disabled', true).addClass('blocked');
        }
    });

    $('#ddlSexo').change(function() {
        setSexoDisplay($(this).val());
    });
});