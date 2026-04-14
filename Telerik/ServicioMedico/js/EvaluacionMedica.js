function showError(msg) {
    $('#msgIcon').html('<i class="fas fa-times-circle" style="color: #e74c3c;"></i>');
    $('#msgTitle').text('Error');
    $('#msgBody').text(msg);
    $('#btnMsgOk').css('background', '#e74c3c');
    $('#msgOverlay').css('display', 'flex');
}

function showInfo(msg) {
    $('#msgIcon').html('<i class="fas fa-info-circle" style="color: #3498db;"></i>');
    $('#msgTitle').text('Informaci\u00f3n');
    $('#msgBody').text(msg);
    $('#btnMsgOk').css('background', '#3498db');
    $('#msgOverlay').css('display', 'flex');
}

function showToast(msg, type) {
    var icon = 'fa-info-circle';
    var color = '#3498db';
    if(type === 'success') { icon = 'fa-check-circle'; color = '#27ae60'; }
    if(type === 'error') { icon = 'fa-times-circle'; color = '#e74c3c'; }
    
    var toastHtml = `
    <div id="medicalToast" style="position:fixed; top:20px; right:20px; z-index:9999; background:white; border-left:5px solid ${color}; padding:15px 20px; border-radius:4px; box-shadow:0 4px 12px rgba(0,0,0,0.15); display:none; align-items:center; min-width:300px; max-width:450px;">
        <i class="fas ${icon}" style="color:${color}; font-size:1.5rem; margin-right:15px;"></i>
        <div style="flex:1;">
            <div style="font-weight:700; color:#333; margin-bottom:2px; font-size:0.9rem;">${type === 'success' ? '\u00c9XITO' : (type === 'error' ? 'ERROR' : 'EXPEDIENTE CL\u00cdNICO')}</div>
            <div style="color:#666; font-size:0.85rem; line-height:1.3;">${msg}</div>
        </div>
        <button onclick="$(this).parent().fadeOut()" style="background:none; border:none; color:#ccc; cursor:pointer; font-size:1.2rem; margin-left:10px;">&times;</button>
    </div>`;
    
    $('#medicalToast').remove();
    $('body').append(toastHtml);
    $('#medicalToast').fadeIn(300);
    
    // Auto-hide after 8 seconds
    setTimeout(function() {
        $('#medicalToast').fadeOut(500);
    }, 8000);
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

function calcImc() {
    var w = parseFloat($('#txtPeso').val());
    var h = parseFloat($('#txtEstatura').val());
    if(w > 0 && h > 0) {
        var imc = w / (h * h);
        $('#txtImc').val(imc.toFixed(1));
        
        // Determinar ClasificaciÃ³n (Escala OMS)
        var desc = "";
        var color = "#2c3e50"; // Default
        
        if (imc < 18.5) {
            desc = "Bajo peso";
            color = "#3498db"; // Azul
        } else if (imc >= 18.5 && imc < 25) {
            desc = "Normal";
            color = "#27ae60"; // Verde
        } else if (imc >= 25 && imc < 30) {
            desc = "Sobrepeso";
            color = "#f39c12"; // Naranja
        } else if (imc >= 30 && imc < 35) {
            desc = "Obesidad Grado I";
            color = "#e67e22"; // Naranja Fuerte
        } else if (imc >= 35 && imc < 40) {
            desc = "Obesidad Grado II";
            color = "#d35400"; // Rojo Intenso
        } else if (imc >= 40) {
            desc = "Obesidad Grado III (M\u00f3rbida)";
            color = "#c0392b"; // Rojo Oscuro
        }
        
        $('#txtImcDescripcion').val(desc.toUpperCase()).css('color', color);
    } else {
        $('#txtImc, #txtImcDescripcion').val('');
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

// Validation Helpers
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

function isOnlyLetters(val) { return /^[A-Za-z\u00C0-\u00FF\s-]+$/.test(val); }
function isOnlyNumbers(val) { return /^\d+$/.test(val); }
function isDecimal(val)     { return /^\d+(\.\d+)?$/.test(val); }

function formatDateForInput(dateVal) {
    if (!dateVal) return "";
    
    try {
        // Handle /Date(1234567890)/
        if (typeof dateVal === 'string' && dateVal.indexOf('/Date(') !== -1) {
            var match = dateVal.match(/\d+/);
            if (match) {
                var timestamp = parseInt(match[0]);
                var date = new Date(timestamp);
                var year = date.getFullYear();
                var month = ('0' + (date.getMonth() + 1)).slice(-2);
                var day = ('0' + date.getDate()).slice(-2);
                return year + '-' + month + '-' + day;
            }
        }
        
        // Handle ISO or yyyy-mm-ddT...
        if (typeof dateVal === 'string' && dateVal.indexOf('T') !== -1) {
            return dateVal.split('T')[0];
        }
        
        // Handle dd/mm/yyyy or yyyy/mm/dd
        if (typeof dateVal === 'string' && dateVal.indexOf('/') !== -1) {
            var parts = dateVal.split('/');
            if (parts.length === 3) {
                if (parts[0].length === 4) return parts[0] + '-' + parts[1] + '-' + parts[2]; // yyyy/mm/dd
                if (parts[2].length === 4) return parts[2] + '-' + ('0' + parts[1]).slice(-2) + '-' + ('0' + parts[0]).slice(-2); // dd/mm/yyyy
            }
        }

        // If already yyyy-mm-dd
        if (typeof dateVal === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(dateVal)) {
            return dateVal;
        }

        return dateVal; // Return as is if format isn't recognized
    } catch (e) {
        console.error("Format date error:", e, dateVal);
        return "";
    }
}

function clearPanelErrors(panelId) {
    $('#panel' + panelId + ' .val-msg').remove();
    $('#panel' + panelId + ' .form-control').css({ 'border-color': '', 'background': '' });
}

// Input mask and formatting logic
function initInputFormatters() {
    // Numbers only
    $(document).on('input', '.val-num', function() { 
        this.value = this.value.replace(/[^0-9]/g, ''); 
    });
    
    // Decimals
    $(document).on('input', '.val-dec', function() { 
        var start = this.selectionStart;
        var oldVal = this.value;
        var newVal = oldVal.replace(/[^0-9.]/g, '').replace(/(\..*?)\..*/g, '$1'); 
        
        if (oldVal !== newVal) {
            this.value = newVal;
            if (this.setSelectionRange) {
                var diff = oldVal.length - newVal.length;
                this.setSelectionRange(start - diff, start - diff);
            }
        }
    });

    // Text only (with spaces and accents)
    $(document).on('input', '.val-text', function() { 
        this.value = this.value.replace(/[^a-zA-Z\u00C0-\u017F\s.]/g, ''); 
    });

    $(document).on('input', '.val-slash-num', function() {
        this.value = this.value.replace(/[^0-9/]/g, '');
    });
}
// medical-form-data.js
// Handles patient data loading and initial form population

var antecedentesHF = [
    "HTA", "ENFERMEDAD CORONARIA", "ACV", "DIABETES", "TIROIDES", 
    "ASMA", "ALERGIAS", "TBC", "ALCOHOL", "EPILEPSIA .", 
    "ENFERM. MENTALES", "MALFORM. CONG&Eacute;NITAS", "C&Aacute;NCER / TUMORALES", "V&Aacute;RICES"
];
var antecedentesPP = [
    "HIPERTENSI&Oacute;N", "QUIR&Uacute;RGICOS", "TRAUM&Aacute;TICOS", "AL&Eacute;RGICOS", "CONG&Eacute;NITOS", 
    "METAB&Oacute;LICOS", "INFECTOCONTAGIOSOS", "TUMORALES", "ENF. RESPIRATORIAS", "MEDICAMENTOS", 
    "TRANSFUSIONALES", "LITIASIS", "HACINAMIENTO", "SERVICIOS: AGUA", "SERVICIOS: DRENAJE", "OTROS ANTECEDENTES"
];

var examSystems = [
    "1. Cabeza", "2. Ojos", "3. Nariz", "4. Boca",
    "5. Dentadura", "6. Faringe", "7. Am\u00edgdalas", "8. Otoscopia",
    "9. Cuello", "10. Columna-espalda", "11. Extremidades", "12. Piel",
    "13. Ap. Respiratorio", "14. Cardiaco", "15. Vascular perif\u00e9rico",
    "16. Abdomen", "17. Neurol\u00f3gico", "18. Genitales", "19. Hernias", "20. Otro"
];

function initForms() {
    // Inicializar cascada de catÃ¡logos geogrÃ¡ficos
    cargarPaises();
    cargarEstadoNacimiento(1); // Cargar estados de MÃ©xico por defecto

    $('#ddlPais').on('change', function() {
        var idPais = $(this).val();
        $('#ddlEstado').html('<option value="">-- Seleccione --</option>').val('');
        $('#ddlMunicipio').html('<option value="">-- Seleccione --</option>').val('');
        $('#ddlColonia').html('<option value="">-- Seleccione --</option>').val('');
        $('#txtCp').val('');
        if(idPais) cargarEstados(idPais);
    });

    $('#ddlEstado').on('change', function() {
        var idEstado = $(this).val();
        $('#ddlMunicipio').html('<option value="">-- Seleccione --</option>').val('');
        $('#ddlColonia').html('<option value="">-- Seleccione --</option>').val('');
        $('#txtCp').val('');
        if(idEstado) cargarMunicipios(idEstado);
    });

    $('#ddlMunicipio').on('change', function() {
        var idMunicipio = $(this).val();
        $('#ddlColonia').html('<option value="">-- Seleccione --</option>').val('');
        $('#txtCp').val('');
        if(idMunicipio) cargarColonias(idMunicipio);
    });

    $('#ddlColonia').on('change', function() {
        var $opt = $(this).find('option:selected');
        var cp = $opt.data('cp');
        var cpId = $opt.data('cp-id');
        if(cp) $('#txtCp').val(cp);
        if(cpId) $('#hdnFkCp').val(cpId);
        else $('#hdnFkCp').val('');
    });

    var $tbHF = $('#tbAntecedentesHF');
    antecedentesHF.forEach(function(item) {
        var row = `<tr>
            <td style="font-weight: 500;">${item}</td>
            <td style="text-align:center;"><input type="checkbox" class="chk-ant chk-hf" data-name="${item}" /></td>
            <td><input type="text" class="form-control ant-det blocked" disabled placeholder="Describa aqu&iacute;..." /></td>
        </tr>`;
        $tbHF.append(row);
    });

    var $tbPP = $('#tbAntecedentesPP');
    antecedentesPP.forEach(function(item) {
        var row = `<tr>
            <td style="font-weight: 500;">${item}</td>
            <td style="text-align:center;"><input type="checkbox" class="chk-ant chk-pp" data-name="${item}" /></td>
            <td><input type="text" class="form-control ant-det blocked" disabled placeholder="Describa aqu&iacute;..." /></td>
        </tr>`;
        $tbPP.append(row);
    });

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

    $tbEx.on('change', '.chk-norm', function() {
        if(this.checked) $(this).closest('tr').find('.chk-anorm').prop('checked', false);
    });
    $tbEx.on('change', '.chk-anorm', function() {
        if(this.checked) $(this).closest('tr').find('.chk-norm').prop('checked', false);
    });
    
    // Toggle handling for HÃ¡bito checkboxes
    $('.toggle-habito').on('change', function() {
        var targetSection = $(this).data('target');
        if(this.checked) {
            $(targetSection).show();
        } else {
            $(targetSection).hide();
            // Optional: reset fields inside target
            $(targetSection).find('input[type=text], input[type=number], select').val('');
        }
    });

    addLaboralRow();
}

function addLaboralRow() {
    if ($('#tbAntecedentesLaborales tr').length >= 3) {
        showError("M\u00e1ximo 3 antecedentes laborales permitidos.");
        return;
    }
    var row = `<tr>
        <td><input type="text" class="form-control lab-emp" placeholder="Empresa" /></td>
        <td><input type="text" class="form-control lab-pue" placeholder="Puesto" /></td>
        <td><input type="text" class="form-control lab-tie" placeholder="Ej. 1 AÃ±o" /></td>
        <td><input type="text" class="form-control lab-age" placeholder="Polvo, ruido, etc." /></td>
        <td><input type="text" class="form-control lab-acc" placeholder="Ninguno" /></td>
        <td><button class="btn-danger" style="padding: 2px 6px; font-size: 0.8rem;" onclick="$(this).closest('tr').remove()"><i class="fas fa-trash"></i></button></td>
    </tr>`;
    $('#tbAntecedentesLaborales').append(row);
}

function loadPatientData(idOrden) {
    $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerDatosPaciente', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idOrden: idOrden }), success: function(r) { var resp = r.d;
        if(resp.success) {
            var p = resp.paciente;
            currentTipo = p.Tipo;

            // --- POBLAR BANNER ---
            $('#pbNombre').text('SERV. MÉDICO — EXAMEN MÉDICO');
            $('#pbEmpresa').text('');
            $('#pbPuesto').text('');
            $('#pbTipoServicio').text('');

            if (p.Tipo === 'EMPLEADO') {
                $('#pbTipo').text('Empleado').removeClass('pb-badge-candidato');
                $('#pbNumEmpSep, #pbNumEmp').show();
                $('#pbNumEmp').text('No. Emp: ' + (p.NumeroEmpleado || '—'));
            } else {
                $('#pbTipo').text('Candidato').addClass('pb-badge-candidato');
            }

            // --- ACTUALIZAR CONSENTIMIENTO CON NOMBRE REAL DE EMPRESA ---
            var empresa = p.Empresa || 'la Empresa';
            if (currentTipoServicio == 3) {
                $('#consentTitle').text('Consentimiento Informado — Examen Toxicológico');
                $('#consentBodyText').html('<p>La empresa <strong>' + empresa + '</strong> informa que se realizará una prueba de detección de consumo de drogas y alcohol, conforme al reglamento interno vigente.</p><p>Los resultados son <strong>confidenciales</strong> y serán utilizados únicamente con fines laborales y de seguridad.</p>');
            } else {
                $('#consentTitle').text('Consentimiento Informado — Examen Médico');
                $('#consentBodyText').html('<p>Por este medio otorgo mi consentimiento a la empresa <strong>' + empresa + '</strong> para la realización de una evaluación médica integral.</p><p>Entiendo que los datos recabados son para uso exclusivo del expediente clínico laboral y serán manejados con estricta confidencialidad.</p><p>Manifiesto que la información proporcionada sobre mis antecedentes es verídica.</p>');
            }
            // No abrir consentimiento aquí: solo debe mostrarse cuando se inicie Antidoping.

            // --- PASO 0 PARA EMPLEADOS ---
            if (resp.esEmpleado) {
                $('#step0').show();
                $('#panel0').show();
                // Ir al paso 0 primero para que el médico vea el historial
                goToStep(0);
                // Cargar historial
                cargarHistorialEmpleado(idOrden);
            }

            // --- LLENAR CAMPOS DEL FORMULARIO ---
            $('#txtNombre').val(p.Nombre || '');
            $('#txtApellidoPaterno').val(p.ApellidoPaterno || '');
            $('#txtApellidoMaterno').val(p.ApellidoMaterno || '');
            $('#txtEdad').val(p.Edad);
            $('#txtPuesto').val(p.Puesto);
            $('#txtArea').val(p.Area);
            $('#txtEmpresa').val(p.Empresa || '');
           
            if(p.Nss) $('#txtNss').val(p.Nss);
            if(p.Escolaridad) {
                var esc = p.Escolaridad.toUpperCase().trim();
                // Normalizaci\u00f3n para el dropdown
                if(esc === 'MEDIA SUPERIOR' || esc === 'BACHILLERATO') esc = 'PREPARATORIA';
                if(esc === 'UNIVERSIDAD' || esc === 'PROFESIONAL') esc = 'LICENCIATURA';
                $('#ddlEscolaridad').val(esc);
            }
            
            if(p.FechaNacimiento) {
                var formattedDate = formatDateForInput(p.FechaNacimiento);
                $('#txtFechaNacimiento').val(formattedDate).trigger('change');
            }
            if(p.Telefono) $('#txtTelefono').val(p.Telefono);
            if(p.Direccion) $('#txtDomicilio').val(p.Direccion);
            if(p.EstadoCivil) {
                $('#ddlEstadoCivil option').filter(function() {
                    return $(this).text().toUpperCase().indexOf(p.EstadoCivil.toUpperCase()) >= 0 || p.EstadoCivil.toUpperCase().indexOf($(this).val().toUpperCase()) >= 0; 
                }).prop('selected', true);
            }
            if(p.TipoSangre) {
                $('#ddlTipoSangre').val(p.FkTipoSangre || '');
            }
            
            if(p.LugarEvaluacion) {
                $('#txtLugarEvaluacion').val(p.LugarEvaluacion);
            }
            
            if(p.Tipo === 'CANDIDATO') {
                $('#txtNombre, #txtApellidoPaterno, #txtApellidoMaterno, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', false);
                $('#txtEscolaridad').prop('readonly', false);
                // Domicilio habilitado
                $('#txtCalle, #txtNumExt, #txtNumInt, #txtCp').prop('readonly', false);
                $('#ddlPais, #ddlEstado, #ddlMunicipio, #ddlColonia').prop('disabled', false);
                $('#ddlSexo').prop('disabled', false);
                $('#secLaborales').show();
            } else {
                $('#txtNombre, #txtApellidoPaterno, #txtApellidoMaterno, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', true);
                $('#txtEscolaridad').prop('readonly', true);
                // Domicilio solo lectura para empleados (sus datos vienen de BD y se editan en mÃ³dulo RH)
                $('#txtCalle, #txtNumExt, #txtNumInt, #txtCp').prop('readonly', true);
                // NOTA: Se desbloquean los selects de zona geogrÃ¡fica para poder utilizarlos y probarlos
                $('#ddlPais, #ddlEstado, #ddlMunicipio, #ddlColonia').prop('disabled', false);
                $('#secLaborales').show();
            }

            // Pre-cargar datos geogrÃ¡ficos del empleado/candidato
            if(p.FkPais) {
                // Esperar a que los paÃ­ses estÃ©n cargados, luego seleccionar y cargar cascada
                var waitPais = setInterval(function(){
                    if($('#ddlPais option').length > 1) {
                        clearInterval(waitPais);
                        $('#ddlPais').val(p.FkPais);
                        // Cargar Estados y seleccionar
                        $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerEstados', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idPais: p.FkPais }), success: function(r) { var resp2 = r.d;
                            if(resp2.success && resp2.data) {
                                var opts = '<option value="">-- Seleccione --</option>';
                                resp2.data.forEach(function(item) {
                                    opts += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                                });
                                $('#ddlEstado').html(opts);
                                if(p.FkEstado) {
                                    $('#ddlEstado').val(p.FkEstado);
                                    // Cargar Municipios
                                    $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerMunicipios', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idEstado: p.FkEstado }), success: function(r) { var resp3 = r.d;
                                        if(resp3.success && resp3.data) {
                                            var opts2 = '<option value="">-- Seleccione --</option>';
                                            resp3.data.forEach(function(item) {
                                                opts2 += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                                            });
                                            $('#ddlMunicipio').html(opts2);
                                            if(p.FkMunicipio) {
                                                $('#ddlMunicipio').val(p.FkMunicipio);
                                                // Cargar Colonias
                                                $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerColonias', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idMunicipio: p.FkMunicipio }), success: function(r) { var resp4 = r.d;
                                                    if(resp4.success && resp4.data) {
                                                        var opts3 = '<option value="">-- Seleccione --</option>';
                                                        resp4.data.forEach(function(item) {
                                                            opts3 += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                                                        });
                                                        $('#ddlColonia').html(opts3);
                                                        if(p.FkColonia) {
                                                            $('#ddlColonia').val(p.FkColonia);
                                                        }
                                                    }
                                                }});
                                            }
                                        }
                                    }});
                                }
                            }
                        }});
                    }
                }, 200);
            }

            // Pre-llenar campos de direcciÃ³n
            if(p.Calle) $('#txtCalle').val(p.Calle);
            if(p.NumExterior) $('#txtNumExt').val(p.NumExterior);
            if(p.NumInterior) $('#txtNumInt').val(p.NumInterior);
            if(p.FkCP) $('#hdnFkCp').val(p.FkCP);
            if(p.CPDesc) $('#txtCp').val(p.CPDesc);

            if(p.Sexo && p.Sexo.trim() !== "") {
                $('#ddlSexo').val(p.Sexo).prop('disabled', true);
                setSexoDisplay(p.Sexo);
            } else {
                $('#ddlSexo').prop('disabled', false).val("");
                setSexoDisplay(""); 
            }

            // LÃ³gica de Expediente ClÃ­nico (Pre-cargar historial)
            if(resp.evaluacionActual) {
                mapearEvaluacionAlFormulario(resp.evaluacionActual);
            } else if(resp.evaluacionPrevia) {
                mapearEvaluacionAlFormulario(resp.evaluacionPrevia, true); // true = es historial
                // NotificaciÃ³n sutil (Toast) en lugar de alerta invasiva
                showToast("Se han pre-cargado los antecedentes y hÃ¡bitos de la Ãºltima evaluaciÃ³n para su verificaciÃ³n.", "info");
            }

        } else {
            showError(resp.message);
        }
    }});
}



// ------ CATALOGOS GEOGRAFICOS ------
function cargarPaises() {
    $.ajax({
        url: 'EvaluacionMedica.aspx/ObtenerPaises', type: 'POST', contentType: 'application/json', success: function(r) { var resp = r.d;
            if(resp.success && resp.data && resp.data.length > 0) {
                var options = '<option value="">-- Seleccione --</option>';
                resp.data.forEach(function(item) {
                    options += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                });
                $('#ddlPais').html(options);
                $('#divGeoError').hide();
            } else {
                var msg = resp.message || 'El catÃ¡logo de PaÃ­ses estÃ¡ vacÃ­o en la base de datos.';
                $('#divGeoError').text('\u26a0 ' + msg).show();
                console.error('ObtenerPaises - respuesta no exitosa:', resp);
            }
        },
        error: function(xhr, status, err) {
            var detail = xhr.responseText ? xhr.responseText.substring(0, 300) : err;
            $('#divGeoError').text('\u274c Error HTTP al cargar PaÃ­ses (' + xhr.status + '): ' + detail).show();
            console.error('ObtenerPaises - error HTTP:', xhr.status, err, xhr.responseText);
        }
    });
}

function cargarEstados(idPais) {
    $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerEstados', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idPais: idPais }), success: function(r) { var resp = r.d;
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
            });
            $('#ddlEstado').html(options);
        }
    }});
}

function cargarEstadoNacimiento(idPais) {
    $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerEstados', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idPais: idPais }), success: function(r) { var resp = r.d;
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Descripcion + '">' + item.Descripcion + '</option>';
            });
            $('#ddlEstadoNacimiento').html(options);
        }
    }});
}

function cargarMunicipios(idEstado) {
    $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerMunicipios', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idEstado: idEstado }), success: function(r) { var resp = r.d;
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
            });
            $('#ddlMunicipio').html(options);
        }
    }});
}

function cargarColonias(idMunicipio) {
    $.ajax({ url: 'EvaluacionMedica.aspx/ObtenerColonias', type: 'POST', contentType: 'application/json', data: JSON.stringify({ idMunicipio: idMunicipio }), success: function(r) { var resp = r.d;
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Id + '" data-cp="' + item.CodigoPostal + '" data-cp-id="' + item.pkCP + '">' + item.Descripcion + '</option>';
            });
            $('#ddlColonia').html(options);
        }
    }});
}
// medical-wizard.js
// Handles navigation and step validation

var currentStep = 1;
var totalSteps = 6;
var currentSexo = '';
var expedienteYaCargado = false;

function goToStep(step) {
    // Paso 0 es especial: no pasa por validaciones
    if (step === 0) {
        $('.step-panel').removeClass('active').hide();
        $('#panel0').addClass('active').show();
        $('.wizard-step').removeClass('active');
        $('#step0').addClass('active');
        currentStep = 0;
        updateButtons();
        window.scrollTo(0, 0);
        return;
    }

    // Si viene desde el paso 0 o avanzando, no valida el paso anterior si es 0
    if (step > currentStep && currentStep > 0) {
        for (var s = currentStep; s < step; s++) {
            if (!validateStep(s)) {
                showError('Debe completar correctamente todos los campos obligatorios del paso ' + s + ' antes de continuar.');
                return;
            }
        }
    }

    $('.step-panel').removeClass('active').hide();
    $('#panel' + step).addClass('active').show();
    
    $('.wizard-step').removeClass('active');
    $('#step' + step).addClass('active');

    currentStep = step;
    updateButtons();
    window.scrollTo(0, 0);
}

function nextStep() {
    if (currentStep === 0) {
        // Desde el expediente, saltar al paso 1 sin validar
        goToStep(1);
        return;
    }
    if (!validateStep(currentStep)) {
        showError('Por favor revise los campos marcados en rojo.');
        return;
    } 
    if(currentStep < totalSteps) goToStep(currentStep + 1);
}

function prevStep() {
    if(currentStep > 1) goToStep(currentStep - 1);
    else if (currentStep === 1 && $('#step0').is(':visible')) goToStep(0); // Volver al expediente si aplica
}

function updateButtons() {
    if (currentStep === 0) {
        $('#btnPrev').css('visibility', 'hidden');
        $('#btnNext').show().text('Continuar a Captura →').html('<i class="fas fa-arrow-right"></i> Continuar a Captura');
        $('#btnFinish').hide();
        return;
    }
    $('#btnNext').html('Siguiente <i class="fas fa-arrow-right"></i>');
    $('#btnPrev').css('visibility', currentStep <= 1 ? 'hidden' : 'visible');
    if(currentStep === totalSteps) {
        $('#btnNext').hide();
        $('#btnFinish').show();
    } else {
        $('#btnNext').show();
        $('#btnFinish').hide();
    }
}

function setSexoDisplay(sexo) {
    if (!sexo) {
        $('#secGineco, #formFem, #secGenito, #formMasc').hide();
        $('#msgSexoPendiente').show();
        currentSexo = '';
        return;
    }

    $('#msgSexoPendiente').hide();
    var s = sexo.toString().toUpperCase().trim();
    
    if (s === 'F' || s.indexOf('FEM') === 0 || s === 'MUJER' || s === '2') {
        currentSexo = 'F';
        $('#secGineco, #formFem').show();
        $('#secGenito, #formMasc').hide();
    } else if (s === 'M' || s.indexOf('MAS') === 0 || s === 'H' || s === 'HOMBRE' || s === '1') {
        currentSexo = 'M';
        $('#secGineco, #formFem').hide();
        $('#secGenito, #formMasc').show();
    } else {
        currentSexo = s;
        $('#secGineco, #formFem, #secGenito, #formMasc').hide();
        $('#msgSexoPendiente').show();
    }
}

function validateStep(step) {
    clearPanelErrors(step);
    var ok = true;
    var firstInvalid = null;

    // Helper to check mandatory text
    function checkReq($el, msg) {
        if (!$el.val() || !$el.val().trim()) {
            markError($el, msg || 'Campo obligatorio');
            if (!firstInvalid) firstInvalid = $el;
            ok = false;
        }
    }

    if (step === 1) {
        checkReq($('#txtLugarEvaluacion'), 'Ingrese el lugar de la evaluaciÃ³n');
        checkReq($('#txtFechaExamen'), 'Ingrese la fecha');
        checkReq($('#txtNombre'), 'Nombre es obligatorio');
        checkReq($('#txtApellidoPaterno'), 'Apellido paterno obligatorio');
        checkReq($('#txtNss'), 'No. IMSS obligatorio');
        checkReq($('#ddlEstadoNacimiento'), 'Seleccione el estado de nacimiento');
        checkReq($('#txtFechaNacimiento'), 'Ingrese fecha de nacimiento');
        checkReq($('#ddlEstadoCivil'), 'Seleccione estado civil');
        checkReq($('#ddlManoDominante'), 'Seleccione mano dominante');
        checkReq($('#txtTelefono'), 'TelÃ©fono obligatorio');
        checkReq($('#ddlPais'), 'Seleccione paÃ­s');
        checkReq($('#ddlEstado'), 'Seleccione estado');
        checkReq($('#ddlMunicipio'), 'Seleccione municipio');
        checkReq($('#ddlColonia'), 'Seleccione colonia');
        checkReq($('#ddlEscolaridad'), 'Seleccione escolaridad');
        if (!$('#txtProfesion').is(':disabled')) {
            checkReq($('#txtProfesion'), 'Indique profesiÃ³n u oficio');
        }
        checkReq($('#ddlSexo'), 'Sexo obligatorio');
        checkReq($('#ddlTipoSangre'), 'Seleccione tipo de sangre');

        var nss = $('#txtNss').val().trim();
        if (nss && nss.length !== 11) {
            markError($('#txtNss'), 'El No. IMSS debe tener 11 dÃ­gitos');
            ok = false;
        }
    }

    if (step === 2) {
        // En antecedentes, al menos las alergias/observaciones deberÃ­an tener algo o "Negado"
        checkReq($('#txtAlergias'), 'Indique alergias u "Observaciones negadas"');
    }

    if (step === 3) {
        if ($('#chkFuma').is(':checked')) {
            checkReq($('#txtAnosFuma'), 'Indique aÃ±os');
            checkReq($('#txtCigarrillos'), 'Indique cigarros/dÃ­a');
        }
        if ($('#chkDrogas').is(':checked')) {
            checkReq($('#txtTipoDrogas'), 'Especifique tipo de droga');
        }
        if ($('#chkAlcohol').is(':checked')) {
            checkReq($('#txtFrecAlcohol'), 'Seleccione frecuencia');
        }
    }

    if (step === 4) {
        var vitals = [
            { id: '#txtSistolica',       label: 'SistÃ³lica' },
            { id: '#txtDiastolica',      label: 'DiastÃ³lica' },
            { id: '#txtFrecCardiaca',    label: 'FC' },
            { id: '#txtFrecRespiratoria',label: 'FR' },
            { id: '#txtPeso',            label: 'Peso' },
            { id: '#txtEstatura',        label: 'Estatura' },
            { id: '#txtTemperatura',     label: 'Temperatura' },
            { id: '#txtGlucosa',         label: 'Glucosa' },
            { id: '#txtOximetria',       label: 'OximetrÃ­a' }
        ];
        
        vitals.forEach(function(v) {
            checkReq($(v.id), v.label + ' obligatorio');
        });

        // Agudeza Visual
        checkReq($('#ddlOdSinLentes'), 'Obligatorio');
        checkReq($('#ddlOiSinLentes'), 'Obligatorio');
        checkReq($('#ddlAoSinLentes'), 'Obligatorio');
        checkReq($('#ddlUsaLentes'), 'Obligatorio');
    }

    if (step === 5) {
        if (currentSexo === 'F') {
            checkReq($('#txtMenarca'), 'Edad menarca obligatoria');
            checkReq($('#txtCiclos'), 'Seleccione ciclo');
            checkReq($('#txtFum'), 'Fecha FUM obligatoria');
        }
    }

    if (step === 6) {
        checkReq($('#txtDiagnostico'), 'DiagnÃ³stico obligatorio');
        checkReq($('#ddlAptitud'), 'Resultado obligatorio');
    }

    if (!ok) {
        var $first = $('#panel' + step + ' .val-msg').first();
        if ($first.length) {
            $('html, body').animate({ scrollTop: $first.offset().top - 120 }, 300);
        }
    }
    if (!ok && firstInvalid && firstInvalid.length) {
        try {
            var el = firstInvalid[0];
            if (el && typeof el.scrollIntoView === 'function') {
                el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            firstInvalid.focus();
        } catch (e) {
            // ignore
        }
    }
    return ok;
}
// Handles collecting data from the UI and saving to the server

function saveExam() {
    if(!$('#ddlAptitud').val()) {
        showError("Debe seleccionar una Aptitud Médica.");
        return;
    }

    if (currentTipo === 'CANDIDATO' && !$('#txtNombre').val().trim()) {
        showError("El campo Nombre Completo es obligatorio para los candidatos según la BD.");
        return;
    }

    cambiosSinGuardar = false; 

    function toIntOrNull(v) {
        if (v === null || v === undefined) return null;
        var s = ('' + v).trim();
        if (!s) return null;
        var n = parseInt(s, 10);
        return isNaN(n) ? null : n;
    }

    function toDecimalOrNull(v) {
        if (v === null || v === undefined) return null;
        var s = ('' + v).trim();
        if (!s) return null;
        s = s.replace(',', '.');
        var n = parseFloat(s);
        return isNaN(n) ? null : n;
    }

    function toDateOrNull(v) {
        if (v === null || v === undefined) return null;
        var s = ('' + v).trim();
        if (!s) return null;
        // ASP.NET WebMethod binder expects ISO-ish date strings for DateTime?
        // Keep yyyy-mm-dd only; otherwise avoid 500 by sending null.
        return /^\d{4}-\d{2}-\d{2}$/.test(s) ? s : null;
    }

    // Build Object
    var model = {
        PkOrdenMedico: idOrden,
        PesoKg: toDecimalOrNull($('#txtPeso').val()),
        AlturaMetros: toDecimalOrNull($('#txtEstatura').val()),
        Imc: toDecimalOrNull($('#txtImc').val()),
        PresionSistolica: toIntOrNull($('#txtSistolica').val()),
        PresionDiastolica: toIntOrNull($('#txtDiastolica').val()),
        Temperatura: toDecimalOrNull($('#txtTemperatura').val()),
        FrecuenciaCardiaca: toIntOrNull($('#txtFrecCardiaca').val()),
        FrecuenciaRespiratoria: toIntOrNull($('#txtFrecRespiratoria').val()),
        Glucosa: toDecimalOrNull($('#txtGlucosa').val()),
        Oximetria: toIntOrNull($('#txtOximetria').val()),
        ImcDescripcion: $('#txtImcDescripcion').val(),
        AparatosSistemas: $('#txtAparatosSistemas').val(), 
        FkAptitudMedica: toIntOrNull($('#ddlAptitud').val()),
        Observaciones: $('#txtDiagnostico').val(), // Maps to Observaciones in View Model
        Recomendaciones: $('#txtRecomendaciones').val(),
        SintomasPaciente: $('#txtSintomas').val(),
        
        Nss: $('#txtNss').val(),
        FechaNacimiento: $('#txtFechaNacimiento').val() || null,
        LugarNacimiento: $('#ddlEstadoNacimiento').val(),
        EstadoCivil: $('#ddlEstadoCivil').val(),
        ManoDominante: $('#ddlManoDominante').val(),
        Telefono: $('#txtTelefono').val(),
        Domicilio: (function() {
            var parts = [];
            if ($('#txtCalle').val()) parts.push($('#txtCalle').val());
            if ($('#txtNumExt').val()) parts.push('#' + $('#txtNumExt').val());
            if ($('#txtNumInt').val()) parts.push('Int.' + $('#txtNumInt').val());
            if ($('#ddlColonia option:selected').val()) parts.push($('#ddlColonia option:selected').text());
            if ($('#ddlMunicipio option:selected').val()) parts.push($('#ddlMunicipio option:selected').text());
            if ($('#ddlEstado option:selected').val()) parts.push($('#ddlEstado option:selected').text());
            return parts.length > 0 ? parts.join(', ') : ($('#txtDomicilio').val() || '');
        })(),
        
        // Catálogos Geográficos
        FkPais: $('#ddlPais').val() ? parseInt($('#ddlPais').val()) : null,
        FkEstado: $('#ddlEstado').val() ? parseInt($('#ddlEstado').val()) : null,
        FkMunicipio: $('#ddlMunicipio').val() ? parseInt($('#ddlMunicipio').val()) : null,
        FkColonia: $('#ddlColonia').val() ? parseInt($('#ddlColonia').val()) : null,
        FkCP: $('#hdnFkCp').val() ? parseInt($('#hdnFkCp').val()) : null,
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
            AnosFumando: toIntOrNull($('#txtAnosFuma').val()),
            CigarrosDiarios: toIntOrNull($('#txtCigarrillos').val()),
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
            LordosisCervical: toIntOrNull($('#ddlLordosisCervical').val()),
            LordosisDorsal:   toIntOrNull($('#ddlLordosisDorsal').val()),
            LordosisLumbar:   toIntOrNull($('#ddlLordosisLumbar').val()),
            CifosisCervical:  toIntOrNull($('#ddlCifosisCervical').val()),
            CifosisDorsal:    toIntOrNull($('#ddlCifosisDorsal').val()),
            CifosisLumbar:    toIntOrNull($('#ddlCifosisLumbar').val()),
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
            EdadMenarca: toIntOrNull($('#txtMenarca').val()),
            FechaUltimaMenstruacion: toDateOrNull($('#txtFum').val()), 
            Ciclos: $('#txtCiclos').val(),
            Gestas: toIntOrNull($('#txtGestas').val()),
            Partos: toIntOrNull($('#txtPartos').val()),
            Cesareas: toIntOrNull($('#txtCesareas').val()),
            Abortos: toIntOrNull($('#txtAbortos').val()),
            MetodoPlanificacion: $('#txtPlanificacion').val(),
            FechaUltimoPapanicolau: toDateOrNull($('#txtPap').val())
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
        url: 'EvaluacionMedica.aspx/GuardarEvaluacion',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ model: model }),
        success: function(r) {
            var resp = r.d;
            if(resp.success) {
                idOrden = resp.pkOrden || idOrden; 
                $('#modalConfirmacionAD').css('display', 'flex');
            } else {
                showError("Error al guardar: " + resp.message);
            }
        },
        error: function(xhr, status, error) {
            var detail = xhr && xhr.responseText ? xhr.responseText : '';
            showError('Ocurrió un error en el servidor. HTTP ' + (xhr ? xhr.status : '') + ': ' + error + (detail ? (' | ' + detail) : ''));
        }
    });
}

/**
 * Mapea los datos de una evaluación (actual o histórica) al formulario.
 * @param {Object} d - Datos de la evaluación (EvaluacionMedicaVm)
 * @param {Boolean} esHistorial - Si es true, omite campos mutables como signos vitales y diagnóstico.
 * Mapea los datos de una evaluaciÃ³n (actual o histÃ³rica) al formulario.
 * @param {Object} d - Datos de la evaluaciÃ³n (EvaluacionMedicaVm)
 * @param {Boolean} esHistorial - Si es true, omite campos mutables como signos vitales y diagnÃ³stico.
 */
function mapearEvaluacionAlFormulario(d, esHistorial) {
    if (!d) return;

    // Solo cargar signos vitales si NO es historial (cargando ediciÃ³n de la misma orden)
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
    if(d.Email) $('#txtEmail').val(d.Email);
    
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

    // Antecedentes (Historia MÃ©dica)
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

    // HÃ¡bitos
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

    // VacunaciÃ³n
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

// medical-antidoping.js
// Handles the antidoping workflow and consent

function continuarAntidoping() {
    $('#modalConfirmacionAD').hide();

    // Bandera global: el usuario decidió continuar al flujo de Antidoping
    // (se usa por el handler de consentimiento en el ASPX).
    window.__antidopingFlow = true;
    
    // Configurar consentimiento para Antidoping antes de mostrarlo
    var em = $('#txtEmpresa').val() || 'la Empresa';
    $('#consentTitle').text('Consentimiento Informado — Examen Toxicológico');
    $('#consentBodyText').html(`
        <p>La empresa <strong>${em}</strong> informa que se realizará una prueba de detección de consumo de drogas y alcohol, conforme al reglamento interno vigente.</p>
        <p>Los resultados son <strong>confidenciales</strong> y serán utilizados únicamente con fines laborales y de seguridad.</p>
    `);
    
    // Reset check
    $('#chkAceptoConsentimiento').prop('checked', false);
    $('#btnAceptoCon').prop('disabled', true);

    $('#consentOverlay').css('display', 'flex').hide().fadeIn(300);
}

function cancelarAntidoping() {
    window.__antidopingFlow = false;
    completarSinAntidoping();
}

// Estos se manejan ahora por las funciones unificadas en el ASPX o JS común
// pero se mantienen para compatibilidad si se llaman específicamente
function validarConsentimiento() {
    toggleConsentOk();
}

function aceptarConsentimiento() {
    $('#consentOverlay').fadeOut(150, function() {
        showSuccess("Consentimiento aceptado. Iniciando prueba Antidoping...", function() {
            if($('#mainWizard').length) $('#mainWizard').hide();
            $('#secAntidoping').fadeIn(400);
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
    window.__antidopingFlow = false;
    cambiosSinGuardar = false;
    $.ajax({
        url: 'EvaluacionMedica.aspx/CompletarSinAntidoping',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ pkOrdenMedico: idOrden }),
        success: function(r) {
            window.location.href = '../RecursosHumanos/DashboardRecursosHumanosSM.aspx';
        },
        error: function() {
            window.location.href = '../RecursosHumanos/DashboardRecursosHumanosSM.aspx';
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
        url: 'EvaluacionMedica.aspx?action=GuardarAntidoping',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function(resp) {
            if (resp.success) {
                showSuccess("Examen Antidoping registrado con \u00e9xito.", function() {
                    window.location.href = '../RecursosHumanos/DashboardRecursosHumanosSM.aspx';
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




// ── Expediente Clínico ────────────────────────────────────────────────────
function cargarHistorialEmpleado(idOrden) {
    $.ajax({
        url: 'EvaluacionMedica.aspx/ObtenerHistorialEmpleado',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ idOrden: idOrden }),
        success: function(r) {
            var resp = r.d;
            if (!resp.success) {
                $('#expedienteContainer').html('<div class="exp-empty"><i class="fas fa-exclamation-triangle"></i> No se pudo cargar el historial.</div>');
                return;
            }

            if (resp.esCandidato || !resp.historial || resp.historial.length === 0) {
                $('#expedienteContainer').html('<div class="exp-empty"><i class="fas fa-folder-open" style="font-size:2rem; display:block; margin-bottom:10px; color:#ccc;"></i>Este paciente no tiene evaluaciones previas registradas en el sistema.</div>');
                return;
            }

            $('#badgeTotalEvals').text(resp.historial.length + ' evaluación(es) previa(s)');

            var html = '';
            resp.historial.forEach(function(ev, idx) {
                var aptClass = (ev.AptitudDesc || '').replace(/\s+/g, '-').toUpperCase();

                var vitals = '';
                vitals += vital('Peso', ev.PesoKg, 'kg');
                vitals += vital('Talla', ev.AlturaMetros, 'm');
                vitals += vital('IMC', ev.Imc, ev.ImcDescripcion ? ' (' + ev.ImcDescripcion + ')' : '');
                vitals += vital('Sistólica', ev.PresionSistolica, 'mmHg');
                vitals += vital('Diastólica', ev.PresionDiastolica, 'mmHg');
                if (ev.Glucosa)   vitals += vital('Glucosa', ev.Glucosa, 'mg/dL');
                if (ev.Oximetria) vitals += vital('Oximetría', ev.Oximetria, '%');

                var tags = '';
                if (ev.AntecedentesPositivos && ev.AntecedentesPositivos.length > 0) {
                    tags = '<div class="expr-section-title"><i class="fas fa-exclamation-circle" style="color:#e74c3c;"></i> Antecedentes Positivos</div><div class="expr-tags">';
                    ev.AntecedentesPositivos.forEach(function(a) { tags += '<span class="expr-tag">' + a + '</span>'; });
                    tags += '</div>';
                }

                var diag = ev.Observaciones
                    ? '<div class="expr-section-title"><i class="fas fa-clipboard-check"></i> Diagnóstico / Observaciones</div><div class="expr-diagnostico">' + ev.Observaciones + '</div>'
                    : '';

                html += '<div class="expr-card">' +
                    '<div class="expr-card-header" onclick="toggleExpedienteCard(this)">' +
                        '<span class="expr-fecha"><i class="fas fa-calendar-check" style="margin-right:6px; color:#aaa;"></i>' + ev.FechaEvaluacion + '</span>' +
                        '<span class="expr-lugar">' + (ev.LugarEvaluacion || '') + '</span>' +
                        '<span class="expr-aptitud ' + aptClass + '">' + (ev.AptitudDesc || '—') + '</span>' +
                        '<i class="fas fa-chevron-down expr-chevron"></i>' +
                    '</div>' +
                    '<div class="expr-body">' +
                        '<div class="expr-vitals-grid">' + vitals + '</div>' +
                        tags + diag +
                    '</div>' +
                '</div>';
            });

            $('#expedienteContainer').html(html);

            // Auto-expandir el primer card
            var $first = $('#expedienteContainer .expr-card-header').first();
            if ($first.length) toggleExpedienteCard($first[0]);
        },
        error: function() {
            $('#expedienteContainer').html('<div class="exp-empty"><i class="fas fa-wifi-slash"></i> Error de conexión al cargar el historial.</div>');
        }
    });
}

function vital(label, val, unit) {
    if (val === null || val === undefined || val === '') return '';
    return '<div class="expr-vital"><label>' + label + '</label><span>' + val + ' <span class="expr-unit">' + (unit || '') + '</span></span></div>';
}

function toggleExpedienteCard(header) {
    var $hdr = $(header);
    var $body = $hdr.next('.expr-body');
    var isOpen = $hdr.hasClass('open');
    if (isOpen) {
        $hdr.removeClass('open');
        $hdr.find('.expr-chevron').removeClass('open');
        $body.slideUp(180);
    } else {
        $hdr.addClass('open');
        $hdr.find('.expr-chevron').addClass('open');
        $body.slideDown(220);
    }
}
