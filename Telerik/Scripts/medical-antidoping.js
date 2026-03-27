// medical-antidoping.js
// Handles the antidoping workflow and consent

function continuarAntidoping() {
    $('#modalConfirmacionAD').hide();
    $('#modalConsentimiento').css('display', 'flex');
}

function cancelarAntidoping() {
    completarSinAntidoping();
}

function validarConsentimiento() {
    var nombreMedico = $('#txtMedicoConsentimiento').val().trim();
    var isChecked = $('#chkAceptoConsentimiento').is(':checked');
    $('#btnAceptoCon').prop('disabled', !(nombreMedico.length > 0 && isChecked));
}

function aceptarConsentimiento() {
    $('#modalConsentimiento').hide();
    showSuccess("Consentimiento aceptado. Iniciando prueba Antidoping...", function() {
        if($('#mainWizard').is(':visible')) {
            $('#mainWizard').slideUp();
            $('.page-header h2').html('<i class="fas fa-flask"></i> Servicio M&eacute;dico &mdash; Antidoping');
        }
        $('#secAntidoping').show();
        window.scrollTo(0,0);
    });
}

function rechazarConsentimiento() {
    if(confirm("Si rechaza el consentimiento, no se podr\u00e1 realizar el Antidoping. La solicitud se marcar\u00e1 como Completada. \u00bfDesea salir?")) {
        completarSinAntidoping();
    }
}

function completarSinAntidoping() {
    cambiosSinGuardar = false;
    $.ajax({
        url: '/ServicioMedico/CompletarSinAntidoping',
        type: 'POST',
        data: { pkOrdenMedico: idOrden },
        success: function(resp) {
            window.location.href = '/ServicioMedico/Index';
        },
        error: function() {
            window.location.href = '/ServicioMedico/Index';
        }
    });
}

function toggleResult(btn, type) {
    var $btn = $(btn);
    if($btn.hasClass('disabled')) return;
    $btn.parent().find('.switch-btn').removeClass('active pos neg');
    if($btn.text() === 'Positivo') {
        $btn.addClass('active pos');
    } else {
        $btn.addClass('active neg');
    }
}

function toggleAplica(btn, drug) {
    var $btn = $(btn);
    var row = $btn.closest('tr');
    if($btn.text() === 'No Aplica') {
        $btn.addClass('active warn').siblings().removeClass('active pos neg').addClass('disabled');
        row.find('.switch-btn').not('.warn').addClass('disabled');
    } else {
        $btn.removeClass('active warn');
        row.find('.switch-btn').removeClass('disabled');
    }
}
