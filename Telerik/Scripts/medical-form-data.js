// medical-form-data.js
// Handles patient data loading and initial form population

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

function initForms() {
    // Inicializar cascada de catálogos geográficos
    cargarPaises();
    cargarEstadoNacimiento(1); // Cargar estados de México por defecto

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
        var cp = $(this).find('option:selected').data('cp');
        if(cp) $('#txtCp').val(cp);
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
    
    // Toggle handling for Hábito checkboxes
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
        <td><input type="text" class="form-control lab-tie" placeholder="Ej. 1 Año" /></td>
        <td><input type="text" class="form-control lab-age" placeholder="Polvo, ruido, etc." /></td>
        <td><input type="text" class="form-control lab-acc" placeholder="Ninguno" /></td>
        <td><button class="btn-danger" style="padding: 2px 6px; font-size: 0.8rem;" onclick="$(this).closest('tr').remove()"><i class="fas fa-trash"></i></button></td>
    </tr>`;
    $('#tbAntecedentesLaborales').append(row);
}

function loadPatientData(idOrden) {
    $.getJSON('/ServicioMedico/ObtenerDatosPaciente', { idOrden: idOrden }, function(resp) {
        if(resp.success) {
            var p = resp.paciente;
            currentTipo = p.Tipo;
            
            $('#txtNombre').val(p.Nombre || '');
            $('#txtApellidoPaterno').val(p.ApellidoPaterno || '');
            $('#txtApellidoMaterno').val(p.ApellidoMaterno || '');
            $('#txtEdad').val(p.Edad);
            $('#txtPuesto').val(p.Puesto);
            $('#txtArea').val(p.Area);
            $('#txtEmpresa').val(p.Empresa || '');
           
            if(p.Nss) $('#txtNss').val(p.Nss);
            if(p.Rfc) $('#txtRfc').val(p.Rfc);
            if(p.Curp) $('#txtCurp').val(p.Curp);
            if(p.Escolaridad) $('#ddlEscolaridad').val(p.Escolaridad).trigger('change');
            
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
                $('#ddlTipoSangre').val(p.FkTipoSangre || '');
            }
            
            if(p.LugarEvaluacion) {
                $('#txtLugarEvaluacion').val(p.LugarEvaluacion);
            }
            
            if(p.Tipo === 'CANDIDATO') {
                $('#txtNombre, #txtApellidoPaterno, #txtApellidoMaterno, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', false);
                $('#txtRfc, #txtCurp, #txtEscolaridad, #txtTieneHijos').prop('readonly', false);
                // Domicilio habilitado
                $('#txtCalle, #txtNumExt, #txtNumInt, #txtCp').prop('readonly', false);
                $('#ddlPais, #ddlEstado, #ddlMunicipio, #ddlColonia').prop('disabled', false);
                $('#ddlSexo').prop('disabled', false);
                $('#secLaborales').show();
            } else {
                $('#txtNombre, #txtApellidoPaterno, #txtApellidoMaterno, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', true);
                $('#txtRfc, #txtCurp, #txtEscolaridad, #txtTieneHijos').prop('readonly', true);
                // Domicilio solo lectura para empleados (sus datos vienen de BD y se editan en módulo RH)
                $('#txtCalle, #txtNumExt, #txtNumInt, #txtCp').prop('readonly', true);
                // NOTA: Se desbloquean los selects de zona geográfica para poder utilizarlos y probarlos
                $('#ddlPais, #ddlEstado, #ddlMunicipio, #ddlColonia').prop('disabled', false);
                $('#secLaborales').show();
            }

            // Pre-cargar datos geográficos del empleado/candidato
            if(p.FkPais) {
                // Esperar a que los países estén cargados, luego seleccionar y cargar cascada
                var waitPais = setInterval(function(){
                    if($('#ddlPais option').length > 1) {
                        clearInterval(waitPais);
                        $('#ddlPais').val(p.FkPais);
                        // Cargar Estados y seleccionar
                        $.getJSON('/ServicioMedico/ObtenerEstados', { idPais: p.FkPais }, function(resp2) {
                            if(resp2.success && resp2.data) {
                                var opts = '<option value="">-- Seleccione --</option>';
                                resp2.data.forEach(function(item) {
                                    opts += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                                });
                                $('#ddlEstado').html(opts);
                                if(p.FkEstado) {
                                    $('#ddlEstado').val(p.FkEstado);
                                    // Cargar Municipios
                                    $.getJSON('/ServicioMedico/ObtenerMunicipios', { idEstado: p.FkEstado }, function(resp3) {
                                        if(resp3.success && resp3.data) {
                                            var opts2 = '<option value="">-- Seleccione --</option>';
                                            resp3.data.forEach(function(item) {
                                                opts2 += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                                            });
                                            $('#ddlMunicipio').html(opts2);
                                            if(p.FkMunicipio) {
                                                $('#ddlMunicipio').val(p.FkMunicipio);
                                                // Cargar Colonias
                                                $.getJSON('/ServicioMedico/ObtenerColonias', { idMunicipio: p.FkMunicipio }, function(resp4) {
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
                                                });
                                            }
                                        }
                                    });
                                }
                            }
                        });
                    }
                }, 200);
            }

            // Pre-llenar campos de dirección
            if(p.Calle) $('#txtCalle').val(p.Calle);
            if(p.NumExterior) $('#txtNumExt').val(p.NumExterior);
            if(p.NumInterior) $('#txtNumInt').val(p.NumInterior);
            if(p.CPDesc) $('#txtCp').val(p.CPDesc);

            if(p.Sexo && p.Sexo.trim() !== "") {
                $('#ddlSexo').val(p.Sexo).prop('disabled', true);
                setSexoDisplay(p.Sexo);
            } else {
                $('#ddlSexo').prop('disabled', false).val("");
                setSexoDisplay(""); 
            }

            // Populate labels for consent and antidoping explicitly
            $('#lblPacienteConsentimiento').text(p.NombreCompleto);
            $('#lblAdNombre').text(p.NombreCompleto);
            $('#lblAdNumEmpleado').text(p.NumeroEmpleado || 'N/A');
            $('#lblAdEmpresa').text(p.Empresa);
            $('#hdrEmpresaAd').text(p.Empresa || 'No especificada');
            $('#lblAdEmpresaConsent').text(p.Empresa || 'el Proyecto');
            $('#lblAdPuesto').text(p.Puesto);
            $('#lblAdIdOrden').text(idOrden);

            if (p.TipoServicioDesc && p.TipoServicioDesc.toLowerCase().indexOf('antidoping') >= 0) {
                 $('#modalConsentimiento').css('display', 'flex');
            }
        } else {
            showError(resp.message);
        }
    });
}

// ------ CATALOGOS GEOGRAFICOS ------
function cargarPaises() {
    $.ajax({
        url: '/ServicioMedico/ObtenerPaises',
        type: 'GET',
        dataType: 'json',
        success: function(resp) {
            if(resp.success && resp.data && resp.data.length > 0) {
                var options = '<option value="">-- Seleccione --</option>';
                resp.data.forEach(function(item) {
                    options += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
                });
                $('#ddlPais').html(options);
                $('#divGeoError').hide();
            } else {
                var msg = resp.message || 'El catálogo de Países está vacío en la base de datos.';
                $('#divGeoError').text('\u26a0 ' + msg).show();
                console.error('ObtenerPaises - respuesta no exitosa:', resp);
            }
        },
        error: function(xhr, status, err) {
            var detail = xhr.responseText ? xhr.responseText.substring(0, 300) : err;
            $('#divGeoError').text('\u274c Error HTTP al cargar Países (' + xhr.status + '): ' + detail).show();
            console.error('ObtenerPaises - error HTTP:', xhr.status, err, xhr.responseText);
        }
    });
}

function cargarEstados(idPais) {
    $.getJSON('/ServicioMedico/ObtenerEstados', { idPais: idPais }, function(resp) {
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
            });
            $('#ddlEstado').html(options);
        }
    });
}

function cargarEstadoNacimiento(idPais) {
    $.getJSON('/ServicioMedico/ObtenerEstados', { idPais: idPais }, function(resp) {
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Descripcion + '">' + item.Descripcion + '</option>';
            });
            $('#ddlEstadoNacimiento').html(options);
        }
    });
}

function cargarMunicipios(idEstado) {
    $.getJSON('/ServicioMedico/ObtenerMunicipios', { idEstado: idEstado }, function(resp) {
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Id + '">' + item.Descripcion + '</option>';
            });
            $('#ddlMunicipio').html(options);
        }
    });
}

function cargarColonias(idMunicipio) {
    $.getJSON('/ServicioMedico/ObtenerColonias', { idMunicipio: idMunicipio }, function(resp) {
        if(resp.success && resp.data) {
            var options = '<option value="">-- Seleccione --</option>';
            resp.data.forEach(function(item) {
                options += '<option value="' + item.Id + '" data-cp="' + item.CodigoPostal + '">' + item.Descripcion + '</option>';
            });
            $('#ddlColonia').html(options);
        }
    });
}
