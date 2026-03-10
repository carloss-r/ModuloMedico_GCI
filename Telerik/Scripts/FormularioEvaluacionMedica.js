var currentStep = 1;
    var totalSteps = 6;
    var cambiosSinGuardar = false;
    // var idOrden is now injected from the cshtml view
    var currentSexo = '';
    var currentTipo = '';

    // ... existing list variables ...
    var antecedentesHF = [
        "HTA", "ENF CORONARIA", "ACV", "DIABETES", "TIROIDES", 
        "ASMA", "ALERGIA", "TBC", "ALCOHOL", "EPILEPSIA", 
        "MENTALES", "CONG\u00c9NITAS", "C\u00c1NCER", "VARICES"
    ];
    var antecedentesPP = [
        "HIPERTENSI\u00d3N", "QUIR\u00daRGICOS", "TRAUM\u00c1TICOS", "AL\u00c9RGICOS", "CONG\u00c9NITOS", 
        "METAB\u00d3LICOS", "INFECCIOSOS", "TUMORALES", "ENF. RESPIRATORIAS", "MEDICAMENTOS", 
        "TRANSFUSIONALES", "LITIASIS", "HACINAMIENTO", "AGUA POTABLE", "ALCANTARILLADO", "OTROS"
    ];
    
    var examSystems = [
        "1. Cabeza", "2. Ojos", "3. Nariz", "4. Boca",
        "5. Dentadura", "6. Faringe", "7. Am\u00edgdalas", "8. Otoscopia",
        "9. Cuello", "10. Columna-espalda", "11. Extremidades", "12. Piel",
        "13. Ap. Respiratorio", "14. Cardiaco", "15. Vascular perif\u00e9rico",
        "16. Abdomen", "17. Neurol\u00f3gico", "18. Genitales", "19. Hernias", "20. Otro"
    ];

    $(document).ready(function() {
        // Detectar cambios en cualquier input para activar la advertencia
        $('input, select, textarea').on('change input', function() {
            cambiosSinGuardar = true;
        });

        // Advertencia al intentar salir
        window.onbeforeunload = function (e) {
            if (cambiosSinGuardar) {
                var message = 'Tiene cambios sin guardar. Si sale de la página, los datos se perderán.';
                e.returnValue = message;
                return message;
            }
        };

        initForms();
        loadPatientData();

        // Calcular Edad desde Fecha de Nacimiento
        $('#txtFechaNacimiento').on('change', function() {
            var dob = new Date($(this).val());
            if(!isNaN(dob)) {
                var today = new Date();
                var age = today.getFullYear() - dob.getFullYear();
                var m = today.getMonth() - dob.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
                    age--;
                }
                $('#txtEdad').val(age);
            }
        });

        // Input Validations
        // Numbers only
        $('.val-num').on('input', function() { 
            this.value = this.value.replace(/[^0-9]/g, ''); 
        });
        
        // Decimals (Cursor aware)
        $('.val-dec').on('input', function() { 
            var start = this.selectionStart;
            var oldVal = this.value;
            var newVal = oldVal.replace(/[^0-9.]/g, '').replace(/(\..*?)\..*/g, '$1'); 
            
            if (oldVal !== newVal) {
                this.value = newVal;
                if (this.type === 'text' || this.type === 'search') {
                    var diff = oldVal.length - newVal.length;
                    var newPos = start - diff;
                    if (newPos < 0) newPos = 0;
                    this.setSelectionRange(newPos, newPos);
                }
            }
        });

        // Text only (Letters + spaces)
        $('.val-text').on('input', function() { 
            this.value = this.value.replace(/[^a-zA-Z\u00C0-\u017F\s]/g, ''); 
        });

        // Slash numbers (e.g. 0/0/0)
        $('.val-slash-num').on('input', function() {
            this.value = this.value.replace(/[^0-9/]/g, '');
        });
    });

    // --- Custom Modal Functions ---
    function showError(msg) {
        $('#msgIcon').html('<i class="fas fa-times-circle" style="color: #e74c3c;"></i>');
        $('#msgTitle').text('Error');
        $('#msgBody').text(msg);
        $('#btnMsgOk').css('background', '#e74c3c');
        $('#msgOverlay').css('display', 'flex');
    }

    function showSuccess(msg, callback) {
        $('#msgIcon').html('<i class="fas fa-check-circle" style="color: #27ae60;"></i>');
        $('#msgTitle').text('\u00c9xito');
        $('#msgBody').text(msg);
        $('#btnMsgOk').css('background', '#27ae60').off('click').click(function() {
            $('#msgOverlay').hide();
            if(callback) callback();
        });
        $('#msgOverlay').css('display', 'flex');
    }

    var confirmCallback = null;
    function showConfirm(msg, callback) {
        $('#confirmBody').text(msg);
        confirmCallback = callback;
        $('#confirmOverlay').css('display', 'flex');
    }

    function handleConfirm(result) {
        $('#confirmOverlay').hide();
        if(confirmCallback) confirmCallback(result);
    }
    // -----------------------------

    function initForms() {
        // Generate Antecedentes Rows
        // Generate Antecedentes Rows
        var $tbHF = $('#tbAntecedentesHF');
        antecedentesHF.forEach(function(item) {
            var row = `<tr>
                <td>${item}</td>
                <td style="text-align:center;"><input type="checkbox" class="chk-ant chk-hf" data-name="${item}" /></td>
            </tr>`;
            $tbHF.append(row);
        });

        var $tbPP = $('#tbAntecedentesPP');
        antecedentesPP.forEach(function(item) {
            var row = `<tr>
                <td>${item}</td>
                <td style="text-align:center;"><input type="checkbox" class="chk-ant chk-pp" data-name="${item}" /></td>
            </tr>`;
            $tbPP.append(row);
        });

        // Generate Physical Exam Rows (con columna Normal + Anormal)
        var $tbEx = $('#tbExamenFisico');
        examSystems.forEach(function(sys) {
            var row = `<tr>
                <td>${sys}</td>
                <td style="text-align:center;"><input type="checkbox" class="chk-norm" checked data-sys="${sys}" /></td>
                <td style="text-align:center;"><input type="checkbox" class="chk-anorm" data-sys="${sys}" /></td>
                <td><input type="text" class="form-control hall-ex" placeholder="Descripci\u00f3n de hallazgos" /></td>
            </tr>`;
            $tbEx.append(row);
        });
        // Alternar Normal/Anormal mutuamente excluyentes
        $tbEx.on('change', '.chk-norm', function() {
            if(this.checked) $(this).closest('tr').find('.chk-anorm').prop('checked', false);
        });
        $tbEx.on('change', '.chk-anorm', function() {
            if(this.checked) $(this).closest('tr').find('.chk-norm').prop('checked', false);
        });
        
        // Populate one empty row for laborales by default
        addLaboralRow();
    }

    function addLaboralRow() {
        var row = `<tr>
            <td><input type="text" class="form-control lab-emp" placeholder="Empresa" /></td>
            <td><input type="text" class="form-control lab-pue" placeholder="Puesto" /></td>
            <td><input type="text" class="form-control lab-tie" placeholder="Ej. 1 Año" /></td>
            <td><input type="text" class="form-control lab-age" placeholder="Polvo, ruido, etc." /></td>
            <td><input type="text" class="form-control lab-acc" placeholder="Ninguno" /></td>
            <td><button class="btn-danger" style="padding: 2px 6px; font-size: 0.8rem;" onclick="$(this).closest('tr').remove()"><i class="fas fa-trash"></i></button></td>
        </tr>`;
        $('#tbAntecedentesLaborales').append(row);
    }

    function loadPatientData() {
        $.getJSON('/ServicioMedico/ObtenerDatosPaciente', { idOrden: idOrden }, function(resp) {
            if(resp.success) {
                var p = resp.paciente;
                currentTipo = p.Tipo;
                
                $('#txtNombre').val(p.NombreCompleto);
                $('#txtEdad').val(p.Edad);
                $('#txtPuesto').val(p.Puesto);
                $('#txtArea').val(p.Area);
                $('#txtEmpresa').val(p.Empresa || '');
               
                if(p.Nss) $('#txtNss').val(p.Nss);
                if(p.Rfc) $('#txtRfc').val(p.Rfc);
                if(p.Curp) $('#txtCurp').val(p.Curp);
                if(p.Escolaridad) $('#ddlEscolaridad').val(p.Escolaridad);
                if(p.TieneHijos !== undefined) {
                    $('#txtTieneHijos').val(p.TieneHijos ? ('Sí (' + (p.NumeroHijos || '0') + ')') : 'No');
                }
                if(p.FechaNacimiento) {
                    $('#txtFechaNacimiento').val(p.FechaNacimiento).trigger('change');
                }
                if(p.Telefono) $('#txtTelefono').val(p.Telefono);
                if(p.Direccion) $('#txtDomicilio').val(p.Direccion);
                if(p.EstadoCivil) {
                    $('#ddlEstadoCivil option').filter(function() {
                        return $(this).text().toUpperCase().indexOf(p.EstadoCivil.toUpperCase()) >= 0 || p.EstadoCivil.toUpperCase().indexOf($(this).val().toUpperCase()) >= 0; 
                    }).prop('selected', true);
                }
                if(p.TipoSangre) {
                    $('#ddlTipoSangre option').filter(function() {
                        return $(this).text().toUpperCase() === p.TipoSangre.toUpperCase(); 
                    }).prop('selected', true);
                }
                
                if(p.Tipo === 'CANDIDATO') {
                    $('#txtNombre, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', false);
                    $('#txtDomicilio, #txtRfc, #txtCurp, #txtEscolaridad, #txtTieneHijos').prop('readonly', false);
                    $('#ddlSexo').prop('disabled', false);
                    $('#secLaborales').show();
                } else {
                    $('#txtNombre, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', true);
                    $('#txtDomicilio, #txtRfc, #txtCurp, #txtEscolaridad, #txtTieneHijos').prop('readonly', true);
                    $('#secLaborales').show();
                }

                if(p.Sexo) {
                    $('#ddlSexo').val(p.Sexo).prop('disabled', true);
                    setSexoDisplay(p.Sexo);
                } else {
                    $('#ddlSexo').prop('disabled', false); 
                }

                // ALWAYS POPULATE ANTIDOPING DATA EXPLICITLY IN CASE USER PROCEEDS LATER
                $('#lblPacienteConsentimiento').text(p.NombreCompleto);
                $('#lblAdNombre').text(p.NombreCompleto);
                $('#lblAdNumEmpleado').text(p.NumeroEmpleado || 'N/A');
                $('#lblAdEmpresa').text(p.Empresa);
                $('#lblAdPuesto').text(p.Puesto);
                $('#lblAdIdOrden').text(idOrden);

                // CHECK IF ANTIDOPING EXCLUSIVO PARA LLEVARLO DIRECTO A LA PANTALLA
                if (p.TipoServicioDesc && p.TipoServicioDesc.toLowerCase().indexOf('antidoping') >= 0) {
                     // Show Consent Modal directly
                     $('#modalConsentimiento').css('display', 'flex');
                }
            } else {
                showError(resp.message);
            }
        });
    }

    $('#ddlSexo').change(function() {
        setSexoDisplay($(this).val());
    });

    function setSexoDisplay(sexo) {
        currentSexo = sexo;
        if(sexo === 'F') {
            $('#secGineco, #formFem').show();
            $('#secGenito, #formMasc').hide();
        } else {
            $('#secGineco, #formFem').hide();
            $('#secGenito, #formMasc').show();
        }
    }

    function calcImc() {
        var w = parseFloat($('#txtPeso').val());
        var h = parseFloat($('#txtEstatura').val());
        if(w > 0 && h > 0) {
            var imc = w / (h * h);
            $('#txtImc').val(imc.toFixed(1));
        }
    }

    function calcularEdad() {
        var fNacStr = $('#txtFechaNacimiento').val();
        if(fNacStr) {
            var fNac = new Date(fNacStr);
            var hoy = new Date();
            var edad = hoy.getFullYear() - fNac.getFullYear();
            var m = hoy.getMonth() - fNac.getMonth();
            if (m < 0 || (m === 0 && hoy.getDate() < fNac.getDate())) {
                edad--;
            }
            $('#txtEdad').val(edad);
        } else {
            $('#txtEdad').val('');
        }
    }

    function goToStep(step) {
        $('.step-panel').removeClass('active');
        $('#panel' + step).addClass('active');
        
        $('.wizard-step').removeClass('active');
        $('#step' + step).addClass('active');

        currentStep = step;
        updateButtons();
        
        // Scroll to top
        window.scrollTo(0, 0);
    }


    // ─── Helpers de validación ───────────────────────────────────────────
    function markError($el, msg) {
        $el.css({ 'border-color': '#e74c3c', 'background': '#fff8f8' });
        if (!$el.next('.val-msg').length) {
            $el.after('<span class="val-msg" style="color:#e74c3c; font-size:0.75rem; margin-top:3px; display:block;">' + msg + '</span>');
        }
    }
    function clearError($el) {
        $el.css({ 'border-color': '', 'background': '' });
        $el.next('.val-msg').remove();
    }
    function isOnlyLetters(val) { return /^[a-zA-ZÀ-ÿ\s]+$/.test(val); }
    function isOnlyNumbers(val) { return /^\d+$/.test(val); }
    function isDecimal(val)     { return /^\d+(\.\d+)?$/.test(val); }

    // Limpiar errores de un panel al entrar
    function clearPanelErrors(panelId) {
        $('#panel' + panelId + ' .val-msg').remove();
        $('#panel' + panelId + ' .form-control').css({ 'border-color': '', 'background': '' });
    }

    // ─── Validación por paso ──────────────────────────────────────────────
    function validateStep(step) {
        clearPanelErrors(step);
        var ok = true;

        if (step === 1) {
            // Nombre (readonly pero debe estar cargado)
            var $nom = $('#txtNombre');
            if (!$nom.val().trim()) { markError($nom, 'El nombre es obligatorio.'); ok = false; }

            // Fecha de Nacimiento
            var $fec = $('#txtFechaNacimiento');
            if (!$fec.val()) { markError($fec, 'Ingrese la fecha de nacimiento.'); ok = false; }

            // Estado Civil
            var $ec = $('#ddlEstadoCivil');
            if (!$ec.val()) { markError($ec, 'Seleccione el estado civil.'); ok = false; }

            // Sexo
            var $sx = $('#ddlSexo');
            if (!$sx.val()) { markError($sx, 'Seleccione el sexo.'); ok = false; }

            // No. IMSS — si se llenó debe ser 11 dígitos numéricos
            var nss = $('#txtNss').val().trim();
            if (nss && !(/^\d{11}$/.test(nss))) {
                markError($('#txtNss'), 'El No. IMSS debe tener exactamente 11 dígitos numéricos.');
                ok = false;
            }

            // Teléfono — si se llenó debe ser numérico 10 dígitos
            var tel = $('#txtTelefono').val().trim();
            if (tel && !(/^\d{10,15}$/.test(tel))) {
                markError($('#txtTelefono'), 'El teléfono debe ser numérico (10-15 dígitos).');
                ok = false;
            }

            // Lugar de Evaluación — requerido
            var $lug = $('#txtLugarEvaluacion');
            if (!$lug.val().trim()) { markError($lug, 'Ingrese el lugar de evaluación.'); ok = false; }
        }

        if (step === 2) {
            // Sin campos obligatorios — Antecedentes es informativo
            // Validar que los textos en la tabla laborales sean texto libre (sin restricción)
        }

        if (step === 3) {
            // Hábitos: si fuma, años de hábito debe ser numérico
            if ($('#chkFuma').is(':checked')) {
                var anos = $('#txtAnosFuma').val().trim();
                if (anos && !isOnlyNumbers(anos)) {
                    markError($('#txtAnosFuma'), 'Solo se permiten números.');
                    ok = false;
                }
                var cig = $('#txtCigarrillos').val().trim();
                if (cig && !isOnlyNumbers(cig)) {
                    markError($('#txtCigarrillos'), 'Solo se permiten números.');
                    ok = false;
                }
            }
        }

        if (step === 4) {
            // Signos vitales — todos obligatorios y numéricos
            var vitals = [
                { id: '#txtSistolica',       label: 'TA Sistólica',    type: 'num' },
                { id: '#txtDiastolica',      label: 'TA Diastólica',   type: 'num' },
                { id: '#txtFrecCardiaca',    label: 'FC',               type: 'num' },
                { id: '#txtFrecRespiratoria',label: 'FR',               type: 'num' },
                { id: '#txtPeso',            label: 'Peso',             type: 'dec' },
                { id: '#txtEstatura',        label: 'Estatura',         type: 'dec' }
            ];
            vitals.forEach(function(v) {
                var $el = $(v.id);
                var val = $el.val().trim();
                if (!val) {
                    markError($el, v.label + ' es obligatorio.');
                    ok = false;
                } else if (v.type === 'num' && !isOnlyNumbers(val)) {
                    markError($el, 'Solo se permiten números enteros.');
                    ok = false;
                } else if (v.type === 'dec' && !isDecimal(val)) {
                    markError($el, 'Solo se permiten números (use punto decimal).');
                    ok = false;
                }
            });

            // Temperatura — opcional pero si se llena debe ser decimal
            var temp = $('#txtTemperatura').val().trim();
            if (temp && !isDecimal(temp)) {
                markError($('#txtTemperatura'), 'Solo se permiten números decimales (ej. 36.5).');
                ok = false;
            }
        }

        if (step === 5) {
            // Si es femenino: validar campos numéricos
            if (currentSexo === 'F') {
                var menarca = $('#txtMenarca').val().trim();
                if (menarca && !isOnlyNumbers(menarca)) {
                    markError($('#txtMenarca'), 'Solo se permiten números.');
                    ok = false;
                }
                var ivsaF = $('#txtIvsaFem').val().trim();
                if (ivsaF && !isOnlyNumbers(ivsaF)) {
                    markError($('#txtIvsaFem'), 'Solo se permiten números.');
                    ok = false;
                }
                ['#txtGestas','#txtPartos','#txtAbortos','#txtCesareas'].forEach(function(id) {
                    var v = $(id).val().trim();
                    if (v && !isOnlyNumbers(v)) {
                        markError($(id), 'Solo se permiten números.');
                        ok = false;
                    }
                });
            }
            // Si es masculino: IVSA numérico
            if (currentSexo === 'M') {
                var ivsaM = $('#txtIvsaMasc').val().trim();
                if (ivsaM && !isOnlyNumbers(ivsaM)) {
                    markError($('#txtIvsaMasc'), 'Solo se permiten números.');
                    ok = false;
                }
            }
        }

        if (step === 6) {
            // RESULTADO obligatorio
            var $apt = $('#ddlAptitud');
            if (!$apt.val()) { markError($apt, 'Debe seleccionar el RESULTADO.'); ok = false; }
        }

        if (!ok) {
            // Scroll al primer error visible
            var $first = $('#panel' + step + ' .val-msg').first();
            if ($first.length) {
                $('html, body').animate({ scrollTop: $first.offset().top - 120 }, 300);
            }
        }
        return ok;
    }

    function nextStep() {
        if (!validateStep(currentStep)) return; // Bloquear si hay errores
        if(currentStep < totalSteps) goToStep(currentStep + 1);
    }


    function prevStep() {
        if(currentStep > 1) goToStep(currentStep - 1);
    }

    function updateButtons() {
        $('#btnPrev').css('visibility', currentStep === 1 ? 'hidden' : 'visible');
        if(currentStep === totalSteps) {
            $('#btnNext').hide();
            $('#btnFinish').show();
        } else {
            $('#btnNext').show();
            $('#btnFinish').hide();
        }
    }

    function saveExam() {
        // Basic Validation
        if(!$('#ddlAptitud').val()) {
            showError("Debe seleccionar una Aptitud M\u00e9dica.");
            return;
        }

        if (currentTipo === 'CANDIDATO' && !$('#txtNombre').val().trim()) {
            showError("El campo Nombre Completo es obligatorio para los candidatos según la BD.");
            return;
        }

        cambiosSinGuardar = false; // Desactivar advertencia

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
            AparatosSistemas: $('#txtAparatosSistemas').val(),
            FkAptitudMedica: $('#ddlAptitud').val(),
            Observaciones: $('#txtObservaciones').val(),
            Recomendaciones: $('#txtRecomendaciones').val(),
            SintomasPaciente: $('#txtSintomas').val(),
            
            // New Demographic Fields
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
            TipoSangre: $('#ddlTipoSangre').val(),
            
            // Candidate Info Update
            NombreCandidato: currentTipo === 'CANDIDATO' ? $('#txtNombre').val() : null,
            PuestoCandidato: currentTipo === 'CANDIDATO' ? $('#txtPuesto').val() : null,
            AreaCandidato: currentTipo === 'CANDIDATO' ? $('#txtArea').val() : null,
            EmpresaCandidato: currentTipo === 'CANDIDATO' ? $('#txtEmpresa').val() : null,
            SexoCandidato: currentTipo === 'CANDIDATO' ? $('#ddlSexo').val() : null,
            // EdadCandidato: $('#txtEdad').val() // We rely on DOB usually, but for display we use text. 
            
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
                DescripcionTiempoLibre: $('#txtTiempoLibre').val(),
                VacunaTetanos: parseInt($('#ddlTetanos').val()) || 0,
                VacunaHepatitis: parseInt($('#ddlHepatitis').val()) || 0,
                VacunaH1N1: $('#chkH1N1').is(':checked')
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

        // Collect Lists
        $('#tbAntecedentesHF tr').each(function() {
            var name = $(this).find('.chk-hf').data('name');
            var checked = $(this).find('.chk-hf').is(':checked');
            if(checked) {
                model.Antecedentes.push({ Categoria: 'Heredo Familiares', NombreCondicion: name, EsPositivo: true, Detalles: '' });
            }
        });

        $('#tbAntecedentesPP tr').each(function() {
            var name = $(this).find('.chk-pp').data('name');
            var checked = $(this).find('.chk-pp').is(':checked');
            if(checked) {
                model.Antecedentes.push({ Categoria: 'Personales Patologicos', NombreCondicion: name, EsPositivo: true, Detalles: '' });
            }
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

        // Collect Gender Spec
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
                Ets: ''
            };
        } else {
            model.DetalleMasculino = {
                PrepucioRetractil:     $('#chkPrepucio').is(':checked'),
                TesticulosDescendidos: $('#chkTesticulos').is(':checked'),
                Fimosis:               $('#chkFimosis').is(':checked'),
                Criptorquidia:         $('#chkCriptorquidia').is(':checked'),
                Varicocele:            $('#chkVaricocele').is(':checked'),
                Hidrocele:             $('#chkHidrocele').is(':checked'),
                Hernia:                $('#chkHernia').is(':checked'),
                Ivsa: $('#txtIvsaMasc').val(),
                Psa:  $('#txtPsa').val()
            };
        }

        $.ajax({
            url: '/ServicioMedico/GuardarEvaluacion',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function(resp) {
                if(resp.success) {
                    // Mostrar Modal de Confirmación para Antidoping
                    idOrden = resp.pkOrden || idOrden; // Ensure we have the ID
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

    function continuarAntidoping() {
        $('#modalConfirmacionAD').hide();
        $('#modalConsentimiento').css('display', 'flex');
    }

    function cancelarAntidoping() {
        // El antidoping se cancela desde la pantalla de antidoping
        // Marcar la orden como Completada (3) antes de salir
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
            // Si venimos de examen medico (wizard to antidoping)
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
            // La evaluación ya fue guardada. Marcar la orden como Completada (3) sin antidoping.
            completarSinAntidoping();
        }
    }

    // Marca la orden como Completada (3) y redirige al Index.
    // Se usa cuando el médico termina la evaluación pero omite el antidoping.
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
                // Si falla el marcado, de todas formas regresamos al index
                window.location.href = '/ServicioMedico/Index';
            }
        });
    }

    function toggleResult(btn, type) {
        var $btn = $(btn);
        $btn.parent().find('.switch-btn').removeClass('active pos neg');
        
        if($btn.text() === 'Positivo') {
            $btn.addClass('active pos');
        } else {
            $btn.addClass('active neg');
        }
    }

    function saveAntidoping() {
        var isPos = function(idx) {
            // idx: 0=coc, 1=thc, 2=anf, 3=met, 4=opi
            return $('.switch-field').eq(idx).find('.switch-btn').eq(1).hasClass('active');
        };

        var formData = new FormData();
        formData.append('PkOrdenMedico', idOrden);
        formData.append('ConsentimientoFirmado', $('#chkConsentimiento').is(':checked'));
        formData.append('ResultadoCocaina', isPos(0));
        formData.append('ResultadoTHC', isPos(1));
        formData.append('ResultadoAnfetaminas', isPos(2));
        formData.append('ResultadoMetanfetaminas', isPos(3));
        formData.append('ResultadoOpiaceos', isPos(4));
        formData.append('Comentarios', $('#txtComentariosAd').val());
        
        var verdict = "NO APTO";
        if(!isPos(0) && !isPos(1) && !isPos(2) && !isPos(3) && !isPos(4)) {
            verdict = "APTO";
        }
        formData.append('VeredictoFinal', verdict);

        var fileInput = $('#fileEvidencia')[0];
        if(fileInput.files.length > 0) {
            formData.append('FileEvidencia', fileInput.files[0]);
        }

        cambiosSinGuardar = false;

        $.ajax({
            url: '/ServicioMedico/GuardarAntidoping',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function(resp) {
                if(resp.success) {
                    showSuccess(resp.message, function() {
                        window.location.href = '/ServicioMedico/Index';
                    });
                } else {
                    showError("Error: " + resp.message);
                }
            },
            error: function(xhr, status, error) {
                 showError("Error al guardar Antidoping: " + error);
            }
        });
    }