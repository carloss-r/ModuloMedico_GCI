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
        AparatosSistemas: $('#txtAparatosSistemas').val(), 
        FkAptitudMedica: $('#ddlAptitud').val(),
        Observaciones: $('#txtDiagnostico').val(), // Maps to Observaciones in View Model
        Recomendaciones: $('#txtRecomendaciones').val(),
        SintomasPaciente: $('#txtSintomas').val(),
        
        Nss: $('#txtNss').val(),
        FechaNacimiento: $('#txtFechaNacimiento').val() || null,
        LugarNacimiento: $('#ddlEstadoNacimiento').val(),
        EstadoCivil: $('#ddlEstadoCivil').val(),
        ManoDominante: $('#ddlManoDominante').val(),
        Telefono: $('#txtTelefono').val(),
        Domicilio: $('#txtDomicilio').val(), // fallback
        
        // Catálogos Geográficos
        FkPais: $('#ddlPais').val() ? parseInt($('#ddlPais').val()) : null,
        FkEstado: $('#ddlEstado').val() ? parseInt($('#ddlEstado').val()) : null,
        FkMunicipio: $('#ddlMunicipio').val() ? parseInt($('#ddlMunicipio').val()) : null,
        FkColonia: $('#ddlColonia').val() ? parseInt($('#ddlColonia').val()) : null,
        FkCP: $('#txtCp').val() ? parseInt($('#txtCp').val()) : null,
        Calle: $('#txtCalle').val(),
        NumExterior: $('#txtNumExt').val(),
        NumInterior: $('#txtNumInt').val(),

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
        
        AgudezaVisual: {
            OdSinLentes: $('#ddlOdSinLentes').val(),
            OiSinLentes: $('#ddlOiSinLentes').val(),
            AoSinLentes: $('#ddlAoSinLentes').val(),
            OdConLentes: $('#ddlOdConLentes').val(),
            OiConLentes: $('#ddlOiConLentes').val(),
            AoConLentes: $('#ddlAoConLentes').val(),
            UsaLentes: $('#ddlUsaLentes').val(),
            ReferenciaVisual: $('#ddlReferenciaVisual').val(),
            Daltonismo: $('#ddlDaltonismo').val()
        },
        
        Antecedentes: [],
        AntecedentesLaborales: [],
        OrdenExamenFisico: [],
        
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
            EscoliosisDobleDerecha:     $('#chkEscDoboDer').is(':checked'),
            EscoliosisDobleIzquierda:   $('#chkEscDoboIzq').is(':checked')
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
        model.OrdenExamenFisico.push({
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
            MetodoPlanificacion: $('#txtPlanificacion').val(),
            FechaUltimoPapanicolau: $('#txtPap').val() || null
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
            Psa:  $('#txtPsa').val(),
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

/**
 * Mapea los datos de una evaluación (actual o histórica) al formulario.
 * @param {Object} d - Datos de la evaluación (EvaluacionMedicaVm)
 * @param {Boolean} esHistorial - Si es true, omite campos mutables como signos vitales y diagnóstico.
 */
function mapearEvaluacionAlFormulario(d, esHistorial) {
    if (!d) return;

    // Solo cargar signos vitales si NO es historial (cargando edición de la misma orden)
    if (!esHistorial) {
        $('#txtPeso').val(d.PesoKg);
        $('#txtEstatura').val(d.AlturaMetros);
        $('#txtImc').val(d.Imc);
        $('#txtSistolica').val(d.PresionSistolica);
        $('#txtDiastolica').val(d.PresionDiastolica);
        $('#txtTemperatura').val(d.Temperatura);
        $('#txtFrecCardiaca').val(d.FrecuenciaCardiaca);
        $('#txtFrecRespiratoria').val(d.FrecuenciaRespiratoria);
        $('#txtGlucosa').val(d.Glucosa);
        $('#txtOximetria').val(d.Oximetria);
        $('#txtImcDescripcion').val(d.ImcDescripcion);
        
        $('#txtSintomas').val(d.SintomasPaciente);
        $('#txtAparatosSistemas').val(d.AparatosSistemas);
        $('#txtDiagnostico').val(d.Observaciones);
        $('#txtRecomendaciones').val(d.Recomendaciones);
        if(d.FkAptitudMedica) $('#ddlAptitud').val(d.FkAptitudMedica);
    }

    // Datos persistentes (Identidad) siempre se cargan
    if(d.Nss) $('#txtNss').val(d.Nss);
    
    if(d.FechaNacimiento) {
        var formattedDate = formatDateForInput(d.FechaNacimiento);
        $('#txtFechaNacimiento').val(formattedDate);
        if(typeof calcularEdad === 'function') calcularEdad();
    }
    
    if(d.LugarNacimiento) {
        var found = false;
        var ln = d.LugarNacimiento.toUpperCase();
        $("#ddlEstadoNacimiento option").each(function() {
            if($(this).text().toUpperCase() == ln) {
                $(this).prop('selected', true);
                found = true;
                return false;
            }
        });
        if(!found) $('#ddlEstadoNacimiento').val(d.LugarNacimiento);
    }
    
    if(d.EstadoCivil) $('#ddlEstadoCivil').val(d.EstadoCivil);
    if(d.ManoDominante) $('#ddlManoDominante').val(d.ManoDominante);
    if(d.Telefono) $('#txtTelefono').val(d.Telefono);
    if(d.Domicilio) $('#txtDomicilio').val(d.Domicilio);
    
    if(d.Escolaridad) {
        var esc = d.Escolaridad.toUpperCase().trim();
        if(esc === 'MEDIA SUPERIOR' || esc === 'BACHILLERATO') esc = 'PREPARATORIA';
        if(esc === 'UNIVERSIDAD' || esc === 'PROFESIONAL') esc = 'LICENCIATURA';
        
        var foundEsc = false;
        $("#ddlEscolaridad option").each(function() {
            var val = $(this).val().toUpperCase();
            var text = $(this).text().toUpperCase();
            if(val == esc || text == esc) {
                $(this).prop('selected', true);
                foundEsc = true;
                return false;
            }
        });
        if(!foundEsc) $('#ddlEscolaridad').val(esc);
    }
    if(d.Profesion) $('#txtProfesion').val(d.Profesion);
    if(d.Alergias) $('#txtAlergias').val(d.Alergias);
    if(d.FkTipoSangre) $('#ddlTipoSangre').val(d.FkTipoSangre);
    if(d.LugarEvaluacion) $('#txtLugarEvaluacion').val(d.LugarEvaluacion);

    // Antecedentes (Historia Médica)
    if (d.Antecedentes && d.Antecedentes.length > 0) {
        d.Antecedentes.forEach(function(a) {
            var $chk = $('.chk-ant[data-name="' + a.NombreCondicion + '"]');
            if ($chk.length > 0) {
                $chk.prop('checked', a.EsPositivo).trigger('change');
                if (a.EsPositivo) {
                    $chk.closest('tr').find('.ant-det').val(a.Detalles);
                }
            }
        });
    }

    // Hábitos
    if (d.Habitos) {
        $('#chkFuma').prop('checked', d.Habitos.Fuma).trigger('change');
        if(d.Habitos.Fuma) {
            $('#txtAnosFuma').val(d.Habitos.AnosFumando);
            $('#txtCigarrillos').val(d.Habitos.CigarrosDiarios);
        }
        $('#chkExFumador').prop('checked', d.Habitos.EsExFumador);
        $('#chkAlcohol').prop('checked', d.Habitos.BebeAlcohol).trigger('change');
        if(d.Habitos.BebeAlcohol) $('#txtFrecAlcohol').val(d.Habitos.FrecuenciaAlcohol);
        $('#chkDrogas').prop('checked', d.Habitos.UsaDrogas).trigger('change');
        if(d.Habitos.UsaDrogas) $('#txtTipoDrogas').val(d.Habitos.TipoDrogas);
        $('#chkDeporte').prop('checked', d.Habitos.HaceDeporte).trigger('change');
        if(d.Habitos.HaceDeporte) $('#txtTipoDeporte').val(d.Habitos.TipoDeporte);
        $('#txtTiempoLibre').val(d.Habitos.DescripcionTiempoLibre);
    }

    // Vacunación
    if (d.Vacunacion) {
        $('#chkTetanos1').prop('checked', d.Vacunacion.TetanosDosis1);
        $('#chkTetanos2').prop('checked', d.Vacunacion.TetanosDosis2);
        $('#chkTetanos3').prop('checked', d.Vacunacion.TetanosDosis3);
        $('#chkHepatitis1').prop('checked', d.Vacunacion.HepatitisDosis1);
        $('#chkHepatitis2').prop('checked', d.Vacunacion.HepatitisDosis2);
        $('#chkH1N1').prop('checked', d.Vacunacion.InfluenzaH1N1);
        $('#txtObsVacunas').val(d.Vacunacion.ObservacionesVacunacion);
    }

    // Columna
    if (d.Columna) {
        $('#ddlLordosisCervical').val(d.Columna.LordosisCervical || 0);
        $('#ddlLordosisDorsal').val(d.Columna.LordosisDorsal || 0);
        $('#ddlLordosisLumbar').val(d.Columna.LordosisLumbar || 0);
        $('#ddlCifosisCervical').val(d.Columna.CifosisCervical || 0);
        $('#ddlCifosisDorsal').val(d.Columna.CifosisDorsal || 0);
        $('#ddlCifosisLumbar').val(d.Columna.CifosisLumbar || 0);
        $('#chkEscDorsalDer').prop('checked', d.Columna.EscoliosisDorsalDerecha);
        $('#chkEscDorsalIzq').prop('checked', d.Columna.EscoliosisDorsalIzquierda);
        $('#chkEscLumbarDer').prop('checked', d.Columna.EscoliosisLumbarDerecha);
        $('#chkEscLumbarIzq').prop('checked', d.Columna.EscoliosisLumbarIzquierda);
        $('#chkEscDoboDer').prop('checked', d.Columna.EscoliosisDobleDerecha);
        $('#chkEscDoboIzq').prop('checked', d.Columna.EscoliosisDobleIzquierda);
        $('#txtObsColumna').val(d.Columna.ObservacionesColumna);
    }

    // Gineco / Masculino
    if (d.DetalleFemenino) {
        $('#txtMenarca').val(d.DetalleFemenino.EdadMenarca);
        $('#txtFum').val(formatDateForInput(d.DetalleFemenino.FechaUltimaMenstruacion));
        $('#txtCiclos').val(d.DetalleFemenino.Ciclos);
        $('#txtGestas').val(d.DetalleFemenino.Gestas);
        $('#txtPartos').val(d.DetalleFemenino.Partos);
        $('#txtCesareas').val(d.DetalleFemenino.Cesareas);
        $('#txtAbortos').val(d.DetalleFemenino.Abortos);
        $('#txtPlanificacion').val(d.DetalleFemenino.MetodoPlanificacion);
        $('#txtPap').val(formatDateForInput(d.DetalleFemenino.FechaUltimoPapanicolau));
    } else if (d.DetalleMasculino) {
        $('#chkPrepucio').prop('checked', d.DetalleMasculino.PrepucioRetractil);
        $('#chkTesticulos').prop('checked', d.DetalleMasculino.TesticulosDescendidos);
        $('#chkFimosis').prop('checked', d.DetalleMasculino.Fimosis);
        $('#chkCriptorquidia').prop('checked', d.DetalleMasculino.Criptorquidia);
        $('#chkVaricocele').prop('checked', d.DetalleMasculino.Varicocele);
        $('#chkHidrocele').prop('checked', d.DetalleMasculino.Hidrocele);
        $('#chkHernia').prop('checked', d.DetalleMasculino.Hernia);
        $('#txtPsa').val(d.DetalleMasculino.Psa);
        $('#txtMpf').val(d.DetalleMasculino.MetodoPlanificacion);
    }
    
    // Signos Vitales y Agudeza Visual Historia
    if (d.PesoKg) $('#txtPeso').val(d.PesoKg);
    if (d.AlturaMetros) $('#txtEstatura').val(d.AlturaMetros);
    if (d.Imc) $('#txtImc').val(d.Imc);
    if (d.ImcDescripcion) $('#txtImcDescripcion').val(d.ImcDescripcion.toUpperCase());
    if (d.PresionSistolica) $('#txtSistolica').val(d.PresionSistolica);
    if (d.PresionDiastolica) $('#txtDiastolica').val(d.PresionDiastolica);
    if (d.Temperatura) $('#txtTemperatura').val(d.Temperatura);
    if (d.Glucosa) $('#txtGlucosa').val(d.Glucosa);
    if (d.Oximetria) $('#txtOximetria').val(d.Oximetria);
    if (d.FrecuenciaCardiaca) $('#txtFrecCardiaca').val(d.FrecuenciaCardiaca);
    if (d.FrecuenciaRespiratoria) $('#txtFrecRespiratoria').val(d.FrecuenciaRespiratoria);
    if (d.SintomasPaciente) $('#txtSintomas').val(d.SintomasPaciente);
    if (d.AparatosSistemas) $('#txtAparatosSistemas').val(d.AparatosSistemas);

    if (d.AgudezaVisual) {
        $('#ddlOdSinLentes').val(d.AgudezaVisual.OdSinLentes);
        $('#ddlOiSinLentes').val(d.AgudezaVisual.OiSinLentes);
        $('#ddlAoSinLentes').val(d.AgudezaVisual.AoSinLentes);
        $('#ddlOdConLentes').val(d.AgudezaVisual.OdConLentes);
        $('#ddlOiConLentes').val(d.AgudezaVisual.OiConLentes);
        $('#ddlAoConLentes').val(d.AgudezaVisual.AoConLentes);
        $('#ddlUsaLentes').val(d.AgudezaVisual.UsaLentes);
        $('#ddlReferenciaVisual').val(d.AgudezaVisual.ReferenciaVisual);
        $('#ddlDaltonismo').val(d.AgudezaVisual.Daltonismo);
    }

    if (d.OrdenExamenFisico && d.OrdenExamenFisico.length > 0) {
        d.OrdenExamenFisico.forEach(function(ef) {
            var sys = ef.SistemaCuerpo;
            $('#tbExamenFisico tr').each(function() {
                if($(this).find('td:first').text() == sys) {
                    $(this).find('.chk-norm').prop('checked', ef.EsNormal).trigger('change');
                    $(this).find('.chk-anorm').prop('checked', !ef.EsNormal).trigger('change');
                    $(this).find('.hall-ex').val(ef.Hallazgos);
                }
            });
        });
    }
}
