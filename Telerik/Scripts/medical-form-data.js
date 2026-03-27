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
                $('#txtDomicilio, #txtRfc, #txtCurp, #txtEscolaridad, #txtTieneHijos').prop('readonly', false);
                $('#ddlSexo').prop('disabled', false);
                $('#secLaborales').show();
            } else {
                $('#txtNombre, #txtApellidoPaterno, #txtApellidoMaterno, #txtPuesto, #txtArea, #txtEmpresa, #txtEdad').prop('readonly', true);
                $('#txtDomicilio, #txtRfc, #txtCurp, #txtEscolaridad, #txtTieneHijos').prop('readonly', true);
                $('#secLaborales').show();
            }

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
