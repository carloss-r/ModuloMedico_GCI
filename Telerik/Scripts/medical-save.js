// medical-save.js
// Handles collecting data from the UI and saving to the server

function saveExam() {
    if(!$('#ddlAptitud').val()) {
        showError("Debe seleccionar una Aptitud M\u00e9dica.");
        return;
    }

    if (currentTipo === 'CANDIDATO' && !$('#txtNombre').val().trim()) {
        showError("El campo Nombre Completo es obligatorio para los candidatos según la BD.");
        return;
    }

    cambiosSinGuardar = false; 

    // Build Object
    var model = {
        PkOrdenMedico: idOrden,
        PesoKg: $('#txtPeso').val(),
        AlturaMetros: $('#txtEstatura').val(),
        Imc: $('#txtImc').val(),
        PresionSistolica: $('#txtSistolica').val(),
        PresionDiastolica: $('#txtDiastolica').val(),
        Temperatura: $('#txtTemperatura').val(),
        FrecuenciaCardiaca: $('#txtFrecCardiaca').val(),
        FrecuenciaRespiratoria: $('#txtFrecRespiratoria').val(),
        Glucosa: $('#txtGlucosa').val(),
        Oximetria: $('#txtOximetria').val(),
        ImcDescripcion: $('#txtImcDescripcion').val(),
        AparatosSistemas: null, 
        FkAptitudMedica: $('#ddlAptitud').val(),
        Observaciones: $('#txtObservaciones').val(),
        Recomendaciones: $('#txtRecomendaciones').val(),
        SintomasPaciente: $('#txtSintomas').val(),
        
        Nss: $('#txtNss').val(),
        FechaNacimiento: $('#txtFechaNacimiento').val() || null,
        LugarNacimiento: $('#txtLugarNacimiento').val(),
        EstadoCivil: $('#ddlEstadoCivil').val(),
        ManoDominante: $('#ddlManoDominante').val(),
        Telefono: $('#txtTelefono').val(),
        Domicilio: $('#txtDomicilio').val(),
        Escolaridad: $('#ddlEscolaridad').val(),
        Profesion: $('#txtProfesion').val(),
        Alergias: $('#txtAlergias').val(),
        FkTipoSangre: $('#ddlTipoSangre').val(),
        LugarEvaluacion: $('#txtLugarEvaluacion').val(),
        
        NombreCandidato: currentTipo === 'CANDIDATO' ? $('#txtNombre').val() : null,
        ApellidoPaternoCandidato: currentTipo === 'CANDIDATO' ? $('#txtApellidoPaterno').val() : null,
        ApellidoMaternoCandidato: currentTipo === 'CANDIDATO' ? $('#txtApellidoMaterno').val() : null,
        PuestoCandidato: currentTipo === 'CANDIDATO' ? $('#txtPuesto').val() : null,
        AreaCandidato: currentTipo === 'CANDIDATO' ? $('#txtArea').val() : null,
        EmpresaCandidato: currentTipo === 'CANDIDATO' ? $('#txtEmpresa').val() : null,
        SexoCandidato: currentTipo === 'CANDIDATO' ? $('#ddlSexo').val() : null,
        
        Habitos: {
            Fuma: $('#chkFuma').is(':checked'),
            AnosFumando: $('#txtAnosFuma').val(),
            CigarrosDiarios: $('#txtCigarrillos').val(),
            EsExFumador: $('#chkExFumador').is(':checked'),
            BebeAlcohol: $('#chkAlcohol').is(':checked'),
            FrecuenciaAlcohol: $('#txtFrecAlcohol').val(),
            UsaDrogas: $('#chkDrogas').is(':checked'),
            TipoDrogas: $('#txtTipoDrogas').val(),
            HaceDeporte: $('#chkDeporte').is(':checked'),
            TipoDeporte: $('#txtTipoDeporte').val(),
            DescripcionTiempoLibre: $('#txtTiempoLibre').val()
        },

        Vacunacion: {
            TetanosDosis1: $('#chkTetanos1').is(':checked'),
            TetanosDosis2: $('#chkTetanos2').is(':checked'),
            TetanosDosis3: $('#chkTetanos3').is(':checked'),
            HepatitisDosis1: $('#chkHepatitis1').is(':checked'),
            HepatitisDosis2: $('#chkHepatitis2').is(':checked'),
            InfluenzaH1N1: $('#chkH1N1').is(':checked'),
            ObservacionesVacunacion: $('#txtObsVacunas').val()
        },
        
        Antecedentes: [],
        AntecedentesLaborales: [],
        ExamenFisico: [],
        
        Columna: {
            LordosisCervical: parseInt($('#ddlLordosisCervical').val()) || 0,
            LordosisDorsal:   parseInt($('#ddlLordosisDorsal').val())   || 0,
            LordosisLumbar:   parseInt($('#ddlLordosisLumbar').val())   || 0,
            CifosisCervical:  parseInt($('#ddlCifosisCervical').val())  || 0,
            CifosisDorsal:    parseInt($('#ddlCifosisDorsal').val())    || 0,
            CifosisLumbar:    parseInt($('#ddlCifosisLumbar').val())    || 0,
            ObservacionesColumna: $('#txtObsColumna').val(),
            EscoliosisDorsalDerecha:   $('#chkEscDorsalDer').is(':checked'),
            EscoliosisDorsalIzquierda: $('#chkEscDorsalIzq').is(':checked'),
            EscoliosisLumbarDerecha:   $('#chkEscLumbarDer').is(':checked'),
            EscoliosisLumbarIzquierda: $('#chkEscLumbarIzq').is(':checked'),
            EscoliosisDoboDerecha:     $('#chkEscDoboDer').is(':checked'),
            EscoliosisDoboIzquierda:   $('#chkEscDoboIzq').is(':checked')
        }
    };

    $('#tbAntecedentesHF tr').each(function() {
        var name = $(this).find('.chk-hf').data('name');
        var checked = $(this).find('.chk-hf').is(':checked');
        var details = $(this).find('.ant-det').val();
        model.Antecedentes.push({ Categoria: 'Heredo Familiares', NombreCondicion: name, EsPositivo: checked, Detalles: details });
    });

    $('#tbAntecedentesPP tr').each(function() {
        var name = $(this).find('.chk-pp').data('name');
        var checked = $(this).find('.chk-pp').is(':checked');
        var details = $(this).find('.ant-det').val();
        model.Antecedentes.push({ Categoria: 'Personales Patologicos', NombreCondicion: name, EsPositivo: checked, Detalles: details });
    });

    $('#tbAntecedentesLaborales tr').each(function() {
        var emp = $(this).find('.lab-emp').val();
        var pue = $(this).find('.lab-pue').val();
        var tie = $(this).find('.lab-tie').val();
        var age = $(this).find('.lab-age').val();
        var acc = $(this).find('.lab-acc').val();
        if (emp || pue || tie || age || acc) {
            model.AntecedentesLaborales.push({
                Empresa: emp,
                Puesto: pue,
                TiempoLaborado: tie,
                AgentesExpuesto: age,
                AccidentesPrevios: acc
            });
        }
    });

    $('#tbExamenFisico tr').each(function() {
        model.ExamenFisico.push({
            SistemaCuerpo: $(this).find('.chk-norm').data('sys'),
            EsNormal: $(this).find('.chk-norm').is(':checked'),
            Hallazgos: $(this).find('.hall-ex').val()
        });
    });

    if(currentSexo === 'F') {
        model.DetalleFemenino = {
            EdadMenarca: parseInt($('#txtMenarca').val()) || null,
            FechaUltimaMenstruacion: $('#txtFum').val() || null, 
            Ciclos: $('#txtCiclos').val(),
            Gestas: parseInt($('#txtGestas').val()) || null,
            Partos: parseInt($('#txtPartos').val()) || null,
            Cesareas: parseInt($('#txtCesareas').val()) || null,
            Abortos: parseInt($('#txtAbortos').val()) || null,
            Ivsa: parseInt($('#txtIvsaFem').val()) || null,
            MetodoPlanificacion: $('#txtPlanificacion').val(),
            FechaUltimoPapanicolau: $('#txtPap').val() || null,
            NumeroHijosEdades: $('#txtNoHijosEdades').val(),
            Ets: $('#txtEts').val()
        };
    } else if (currentSexo === 'M') {
        model.DetalleMasculino = {
            PrepucioRetractil:     $('#chkPrepucio').is(':checked'),
            TesticulosDescendidos: $('#chkTesticulos').is(':checked'),
            Fimosis:               $('#chkFimosis').is(':checked'),
            Criptorquidia:         $('#chkCriptorquidia').is(':checked'),
            Varicocele:            $('#chkVaricocele').is(':checked'),
            Hidrocele:             $('#chkHidrocele').is(':checked'),
            Hernia:                $('#chkHernia').is(':checked'),
            Ivsa: parseInt($('#txtIvsaMasc').val()) || null,
            Psa:  $('#txtPsa').val(),
            NumeroHijosEdades: $('#txtTieneHijos').val(),
            MetodoPlanificacion: $('#txtMpf').val()
        };
    }

    $.ajax({
        url: '/ServicioMedico/GuardarEvaluacion',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(model),
        success: function(resp) {
            if(resp.success) {
                idOrden = resp.pkOrden || idOrden; 
                $('#modalConfirmacionAD').css('display', 'flex');
            } else {
                showError("Error al guardar: " + resp.message);
            }
        },
        error: function(xhr, status, error) {
            showError("Ocurri\u00f3 un error en el servidor: " + error);
        }
    });
}
