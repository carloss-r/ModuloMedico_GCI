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
    var isChecked = $('#chkAceptoConsentimiento').is(':checked');
    $('#btnAceptoCon').prop('disabled', !isChecked);
}

function aceptarConsentimiento() {
    $('#modalConsentimiento').fadeOut(150, function() {
        showSuccess("Consentimiento aceptado. Iniciando prueba Antidoping...", function() {
            // Aseguramos que el wizard médico esté oculto y la sección de antidoping visible
            if($('#mainWizard').length) $('#mainWizard').hide();
            
            $('#secAntidoping').fadeIn(400);
            
            // Garantizar visibilidad de elementos internos (Fix para 'pasmado' o blanco)
            $('.ad-container').show().css('visibility', 'visible');
            
            window.scrollTo(0,0);
        });
    });
}


function rechazarConsentimiento() {
    showConfirm("Al rechazar el consentimiento, no se podr\u00e1 realizar el Antidoping. La solicitud se marcar\u00e1 como completada solo con el examen m\u00e9dico. \u00bfDesea salir?", function(res) {
        if(res) completarSinAntidoping();
    });
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
    if($btn.closest('.switch-field').find('.res-btns').hasClass('disabled')) return;
    
    $btn.parent().find('.switch-btn').removeClass('active pos neg');
    if($btn.text() === 'Positivo') {
        $btn.addClass('active pos');
    } else {
        $btn.addClass('active neg');
    }
}

function toggleAplicaRow(chk) {
    var $chk = $(chk);
    var $container = $chk.closest('.switch-field');
    var $resBtns = $container.find('.res-btns');
    
    if(!chk.checked) {
        $resBtns.addClass('disabled').css('opacity', '0.5');
        // $resBtns.find('.switch-btn').removeClass('active pos neg');
    } else {
        $resBtns.removeClass('disabled').css('opacity', '1');
        // Default Negativo
        if (!$resBtns.find('.switch-btn.active').length) {
            $resBtns.find('.switch-btn').first().addClass('active neg');
        }
    }
}

function saveAntidoping() {
    var formData = new FormData();
    formData.append('PkOrdenMedico', idOrden);
    formData.append('ConsentimientoFirmado', $('#chkAceptoConsentimiento').is(':checked'));
    formData.append('Comentarios', $('#txtComentariosAd').val());

    // Evaluate final verdict based on any positive drug result
    var hasPositive = false;
    $('.switch-btn.active.pos').length > 0 ? hasPositive = true : hasPositive = false;
    formData.append('VeredictoFinal', hasPositive ? 'POSITIVO' : 'NEGATIVO');

    // Evidence
    var $file = $('#fileEvidencia');
    if ($file.length > 0 && $file[0].files.length > 0) {
        formData.append('FileEvidencia', $file[0].files[0]);
    }

    // Data Drugs
    var drugMappings = [
        { code: 'alc', name: 'Alcohol' },
        { code: 'coc', name: 'Cocaina' },
        { code: 'thc', name: 'THC' },
        { code: 'anf', name: 'Anfetaminas' },
        { code: 'met', name: 'Metanfetaminas' },
        { code: 'opi', name: 'Opiaceos' },
        { code: 'mfn', name: 'Metilfenidato' },
        { code: 'fen', name: 'Fentanilo' },
        { code: 'bzd', name: 'Benzodiacepinas' }
    ];

    drugMappings.forEach(function(d) {
        var $row = $('[data-drug="' + d.code + '"]');
        var aplica = $row.find('.chk-aplica').is(':checked');
        var result = $row.find('.res-btns .switch-btn.active').text() === 'Positivo';
        formData.append('Aplica' + d.name, aplica);
        formData.append('Resultado' + d.name, result);
    });

    $.ajax({
        url: '/ServicioMedico/GuardarAntidoping',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function(resp) {
            if (resp.success) {
                showSuccess("Examen Antidoping registrado con \u00e9xito.", function() {
                    window.location.href = '/ServicioMedico/Index';
                });
            } else {
                showError("No se pudo completar el guardado: " + resp.message);
            }
        },
        error: function(xhr, status, error) {
            showError("Error de conexi\u00f3n al guardar el antidoping: " + error);
        }
    });
}
