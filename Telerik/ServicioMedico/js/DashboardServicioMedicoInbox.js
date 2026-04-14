/* Lógica de MédicaDashboardServicioMedico.aspx */
$(document).ready(function () {
    paginaActual = 1;
    registrosPorPagina = 25;
    totalRegistrosGlobal = 0;

    cargarInicial();

    var lastOnline = navigator.onLine;
    function updateConnectionStatus() {
        var $status = $('#connectionStatus');
        var $text = $('#connectionStatusText');
        var isOnline = navigator.onLine;
        if (isOnline === lastOnline) return;
        lastOnline = isOnline;

        if (!isOnline) {
            $status.css({ display: 'block', background: '#f8d7da', borderLeftColor: '#dc3545', color: '#721c24' });
            $text.text('Sin conexión a Internet. Algunas funciones no estarán disponibles.');
        } else {
            $status.css({ display: 'block', background: '#d4edda', borderLeftColor: '#28a745', color: '#155724' });
            $text.text('Conexión restaurada. Recargando datos...');
            setTimeout(function () {
                $status.hide();
                aplicarFiltros(true);
            }, 1200);
        }
    }

    window.addEventListener('online', updateConnectionStatus);
    window.addEventListener('offline', updateConnectionStatus);
    if (!navigator.onLine) updateConnectionStatus();
});

var paginaActual = 1;
var registrosPorPagina = 25;
var totalRegistrosGlobal = 0;
var DEFAULT_TIMEOUT = 30000;
var DEFAULT_RETRIES = 2;

$.ajaxSetup({ timeout: DEFAULT_TIMEOUT, cache: false });

function getFriendlyErrorMessage(xhr, status) {
    if (status === 'timeout') return 'La solicitud tardó demasiado. Intente nuevamente.';
    if (xhr && xhr.status === 401) return 'Sesión expirada. Recargue la página.';
    if (xhr && xhr.status === 404) return 'Servicio no disponible (404).';
    if (xhr && xhr.status === 500) return 'Error interno del servidor (500).';
    return 'Error de conexión. Verifique su internet e intente nuevamente.';
}

function apiCall(opts) {
    var retriesLeft = typeof opts.retries === 'number' ? opts.retries : DEFAULT_RETRIES;
    var requestConfig = {
        url: opts.url,
        type: opts.type || 'POST',
        data: opts.data ? JSON.stringify(opts.data) : undefined,
        contentType: opts.contentType || 'application/json; charset=utf-8',
        dataType: opts.dataType || 'json',
        timeout: opts.timeout || DEFAULT_TIMEOUT,
        success: function (response) {
            if (typeof opts.onSuccess === 'function') opts.onSuccess(response);
        },
        error: function (xhr, status, error) {
            if ((status === 'timeout' || (xhr && xhr.status === 0)) && retriesLeft > 0) {
                retriesLeft--;
                setTimeout(function () { $.ajax(requestConfig); }, 700);
                return;
            }

            var msg = getFriendlyErrorMessage(xhr, status);
            console.error('AJAX Error:', opts.url, status, error);
            if (typeof opts.onError === 'function') {
                opts.onError(xhr, status, error, msg);
            } else {
                alert(msg);
            }
        }
    };

    $.ajax(requestConfig);
}

function renderTableError(msg) {
    $('#tbodySolicitudes').html(
        '<tr><td colspan="10" class="no-data"><i class="fas fa-exclamation-circle"></i> ' +
        msg +
        ' <button type="button" class="btn-gci btn-gci-sm btn-gci-primary" onclick="aplicarFiltros(true)" style="margin-left:10px;">' +
        '<i class="fas fa-sync"></i> Reintentar</button></td></tr>'
    );
}

function cargarInicial() {
    aplicarFiltros(true);
}

function verEvaluacionPreview(idOrden) {
    apiCall({
        url: 'DashboardServicioMedico.aspx/ObtenerEvaluacionPreview',
        data: { id: idOrden },
        onSuccess: function(r){
            var resp = (r && r.d !== undefined) ? r.d : r;
            if (typeof resp === 'string') {
                try { resp = JSON.parse(resp); } catch (e) { }
            }
            if(!resp || !resp.success) {
                showMsg('No disponible', (resp && resp.message) ? resp.message : 'No fue posible cargar la evaluación.', 'fa-exclamation-triangle');
                return;
            }
            $('#modalPreviewEvaluacion').addClass('active');
            fillEvaluacionPreview(resp.orden || {}, resp.evaluacion || {});
        },
        onError: function(xhr, status, err, msg){
            showMsg('Error', msg, 'fa-times-circle');
        }
    });
}

function fillEvaluacionPreview(orden, ev){
    orden = orden || {};
    ev = ev || {};

    function toText(v) {
        return (v === null || v === undefined) ? '' : ('' + v);
    }

    function toUpper(v) {
        return toText(v).toUpperCase();
    }

    function toNumber(v) {
        var n = parseInt(v, 10);
        return isNaN(n) ? null : n;
    }

    function normalizeKey(v) {
        var txt = toUpper(v).replace(/^\d+\.?\s*/, '').trim();
        if (txt.normalize) {
            txt = txt.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
        }
        txt = txt.replace(/[^A-Z0-9\s\.:-]/g, '');
        txt = txt.replace(/\s+/g, ' ').trim();
        return txt;
    }

    function safeSection(name, fn) {
        try {
            fn();
        } catch (err) {
            console.error('Error renderizando sección del preview:', name, err);
        }
    }

    function antecedentePositivo(map, label) {
        var key = normalizeKey(label);
        if (map[key]) return true;
        var keys = Object.keys(map);
        for (var i = 0; i < keys.length; i++) {
            if (!map[keys[i]]) continue;
            var k = keys[i];
            if (
                (key.indexOf('ENF CORONARIA') >= 0 && k.indexOf('CORONARIA') >= 0) ||
                (key.indexOf('MENTALES') >= 0 && k.indexOf('MENTAL') >= 0) ||
                (key.indexOf('CONGENITAS') >= 0 && k.indexOf('CONGEN') >= 0) ||
                (key.indexOf('HIPERTENSION') >= 0 && (k.indexOf('HIPERTENSION') >= 0 || k.indexOf('HTA') >= 0)) ||
                (key.indexOf('QUIRURGICOS') >= 0 && k.indexOf('QUIRURG') >= 0) ||
                (key.indexOf('TRAUMATICOS') >= 0 && k.indexOf('TRAUMAT') >= 0) ||
                (key.indexOf('ALERGICOS') >= 0 && k.indexOf('ALERG') >= 0) ||
                (key.indexOf('CONGENITOS') >= 0 && k.indexOf('CONGEN') >= 0) ||
                (key.indexOf('METABOLICOS') >= 0 && k.indexOf('METABOL') >= 0) ||
                (key.indexOf('INFECCIOSOS') >= 0 && (k.indexOf('INFECTO') >= 0 || k.indexOf('INFECC') >= 0)) ||
                (key.indexOf('AGUA POTABLE') >= 0 && k.indexOf('AGUA') >= 0) ||
                (key.indexOf('ALCANTARILLADO') >= 0 && (k.indexOf('DRENAJE') >= 0 || k.indexOf('ALCANTARILL') >= 0)) ||
                (key.indexOf('OTROS') >= 0 && k.indexOf('OTROS') >= 0)
            ) {
                return true;
            }
        }
        return false;
    }

    function calcularEdad(fechaNacimientoRaw) {
        var dt = null;
        var s = toText(fechaNacimientoRaw);
        var m = s.match(/\/Date\((\d+)\)\//);
        if (m) {
            dt = new Date(parseInt(m[1], 10));
        } else if (s) {
            dt = new Date(s);
        }
        if (!dt || isNaN(dt.getTime())) return '';
        var now = new Date();
        var edad = now.getFullYear() - dt.getFullYear();
        var mes = now.getMonth() - dt.getMonth();
        if (mes < 0 || (mes === 0 && now.getDate() < dt.getDate())) edad--;
        return edad > 0 ? edad : '';
    }

    safeSection('base', function () {
        $('#prevFolio').text(toText(orden.FolioDisplay) || '—');
        $('#prevOrden').text(toText(orden.PkOrdenMedico) || '—');
        $('#prevNombreTrabajador').text(toText(orden.NombrePersona));
    });

    safeSection('generales', function () {
        $('#prevLugarFecha').text(trunc((toText(ev.LugarEvaluacion) || '') + (orden.FechaOrdenFormateada ? ('  ' + orden.FechaOrdenFormateada) : ''), 45));
        $('#prevCargo').text(trunc(toText(orden.PuestoCandidato || ev.Puesto), 25));
        $('#prevNombre').text(trunc(toText(orden.NombrePersona), 35));
        $('#prevNss').text(trunc(toText(ev.Nss || orden.NssCandidato), 15));
        $('#prevNacimiento').text(formatDate(ev.FechaNacimiento) || '');
        $('#prevEdad').text(toText(ev.Edad || calcularEdad(ev.FechaNacimiento)));
        $('#prevLugarNac').text(trunc(toText(ev.LugarNacimiento), 20));
        $('#prevTelefono').text(trunc(toText(ev.Telefono), 15));
        $('#prevDomicilio').text(trunc(toText(ev.Domicilio), 50));
        $('#prevMano').text(trunc(toText(ev.ManoDominante), 10));
        $('#prevProfesion').text(trunc(toText(ev.Profesion), 25));
        $('#prevTipoSangre').text(trunc(toText(ev.TipoSangreDesc || getTipoSangre(ev.FkTipoSangre)), 5));
    });

    safeSection('checkboxes-demograficos', function () {
        var sexo = toUpper(orden.SexoCandidato || ev.Sexo).trim();
        $('#prevSexoM').toggleClass('filled', sexo === 'M' || sexo.indexOf('MASC') >= 0 || sexo === '1');
        $('#prevSexoF').toggleClass('filled', sexo === 'F' || sexo.indexOf('FEM') >= 0 || sexo === '2');

        var ec = toUpper(ev.EstadoCivil).trim();
        $('#prevEcSoltero').toggleClass('filled', ec.indexOf('SOLTER') >= 0 || ec === '1');
        $('#prevEcCasado').toggleClass('filled', ec.indexOf('CASAD') >= 0 || ec === '2');
        $('#prevEcUnion').toggleClass('filled', ec.indexOf('UNION') >= 0 || ec === '3');
        $('#prevEcSeparado').toggleClass('filled', ec.indexOf('SEPAR') >= 0 || ec.indexOf('DIVOR') >= 0 || ec.indexOf('VIUD') >= 0 || ec === '4');

        var esc = toUpper(ev.Escolaridad);
        $('#prevNaPrimaria').toggleClass('filled', esc.indexOf('PRIM') >= 0);
        $('#prevNaSecundaria').toggleClass('filled', esc.indexOf('SECUN') >= 0);
        $('#prevNaMedia').toggleClass('filled', esc.indexOf('MEDIA') >= 0 || esc.indexOf('PREPA') >= 0 || esc.indexOf('BACH') >= 0);
        $('#prevNaUniversidad').toggleClass('filled', esc.indexOf('UNIV') >= 0 || esc.indexOf('LIC') >= 0 || esc.indexOf('POS') >= 0);

        var isCand = toUpper(orden.TipoServicioDesc).indexOf('INGRES') >= 0 || toUpper(orden.Modalidad).indexOf('INGRES') >= 0;
        $('#prevExIngreso').toggleClass('filled', isCand);
        $('#prevExPeriodico').toggleClass('filled', !isCand);
    });

    // Helper para truncar texto
    function trunc(txt, max){
        if(!txt) return '';
        txt = txt + '';
        if(txt.length <= max) return txt;
        return txt.substring(0, max - 3) + '...';
    }

    // Antecedentes - separar por categoría
    var ahf = {};
    var app = {};
    safeSection('antecedentes-map', function () {
        if (ev.Antecedentes && ev.Antecedentes.length) {
            ev.Antecedentes.forEach(function (a) {
                if (!a || !a.NombreCondicion) return;
                var key = normalizeKey(a.NombreCondicion);
                var cat = normalizeKey(a.Categoria);
                var isAhf = cat.indexOf('HEREDO') >= 0 || cat.indexOf('FAMILIAR') >= 0 ||
                    key.indexOf('HTA') >= 0 || key.indexOf('DIABETES') >= 0 || key.indexOf('CORONARIA') >= 0 ||
                    key.indexOf('ACV') >= 0 || key.indexOf('TIROIDES') >= 0 || key.indexOf('ASMA') >= 0 ||
                    key.indexOf('TBC') >= 0 || key.indexOf('EPILEPSIA') >= 0 || key.indexOf('MENTAL') >= 0 ||
                    key.indexOf('ALCOHOL') >= 0 || key.indexOf('CONGEN') >= 0 || key.indexOf('CANCER') >= 0 ||
                    key.indexOf('VARICES') >= 0 || key.indexOf('ALERGIA') >= 0;

                if (isAhf) ahf[key] = !!a.EsPositivo;
                else app[key] = !!a.EsPositivo;
            });
        }
    });

    // Antecedentes Heredo-Familiares - tabla 5 columnas (3 filas)
    var ahfRows = [
        ['HTA','DIABETES','ALERGIA','EPILEPSIA','CANCER'],
        ['ENF CORONARIA','TIROIDES','TBC','MENTALES','VARICES'],
        ['ACV','ASMA','ALCOHOL','CONGENITAS','']
    ];
    safeSection('antecedentes-ahf', function () {
        var ahfHtml = '<table class="ant-table">';
        ahfRows.forEach(function(row){
            ahfHtml += '<tr>';
            row.forEach(function(item){
                if(item){
                    var isPos = antecedentePositivo(ahf, item);
                    ahfHtml += '<td style="width:20%; vertical-align:top;"><div class="ant-item"><span class="underline">' + (isPos ? 'X' : '') + '</span>' + item + '</div></td>';
                } else {
                    ahfHtml += '<td style="width:20%;"></td>';
                }
            });
            ahfHtml += '</tr>';
        });
        ahfHtml += '</table>';
        $('#prevAhfContainer').html(ahfHtml);
    });

    // Antecedentes Personales Patológicos - tabla 4 columnas
    var appRows = [
        ['HIPERTENSION','CONGENITOS','ENF. RESPIRATORIAS','HACINAMIENTO'],
        ['QUIRURGICOS','METABOLICOS','MEDICAMENTOS','AGUA POTABLE'],
        ['TRAUMATICOS','INFECCIOSOS','TRANSFUSIONALES','ALCANTARILLADO'],
        ['ALERGICOS','TUMORALES','LITIASIS','OTROS:']
    ];
    safeSection('antecedentes-app', function () {
        var appHtml = '<table class="ant-table">';
        appRows.forEach(function(row){
            appHtml += '<tr>';
            row.forEach(function(item){
                var isPos = antecedentePositivo(app, item);
                appHtml += '<td style="width:25%; vertical-align:top;"><div class="ant-item"><span class="underline">' + (isPos ? 'X' : '') + '</span>' + item + '</div></td>';
            });
            appHtml += '</tr>';
        });
        var obsTxt = trunc(ev.Observaciones, 100);
        appHtml += '<tr><td colspan="4" style="font-size:8px; padding:2px 4px;"><strong>Observaciones:</strong> ' + escapeHtml(obsTxt) + '</td></tr>';
        appHtml += '<tr><td colspan="4" style="font-size:8px; padding:2px 4px;"><strong>ANTECEDENTES PERSONALES NO PATOLOGICOS:</strong></td></tr>';
        appHtml += '</table>';
        $('#prevAppContainer').html(appHtml);
    });

    safeSection('laborales', function () {
        if(ev.AntecedentesLaborales && ev.AntecedentesLaborales.length){
            var lab = ev.AntecedentesLaborales[0] || {};
            $('#prevLabEmpresa').text(trunc(toText(lab.Empresa), 25));
            $('#prevLabTiempo').text(trunc(toText(lab.TiempoLaborado), 15));
            $('#prevLabPuesto').text(trunc(toText(lab.Puesto), 20));
            $('#prevLabAgentes').text(trunc(toText(lab.AgentesExpuesto), 20));
            $('#prevLabAccidentes').text(trunc(toText(lab.AccidentesPrevios), 15));
        } else {
            $('#prevLabEmpresa,#prevLabTiempo,#prevLabPuesto,#prevLabAgentes,#prevLabAccidentes').text('');
        }
    });

    safeSection('habitos', function () {
        var hab = ev.Habitos || {};
        var habHtml = '';
        habHtml += '<div class="hab-row"><span class="cb' + (hab.Fuma ? ' filled' : '') + '"></span><span style="width:50px;">Fuma:</span>';
        habHtml += '<strong>Años de hábito</strong>&nbsp;<span class="hab-line">' + toText(hab.AnosFumando) + '</span>';
        habHtml += '&nbsp;<strong>No. Cigarros/día:</strong>&nbsp;<span class="hab-line">' + toText(hab.CigarrosDiarios) + '</span>';
        habHtml += '&nbsp;<strong>EX</strong>&nbsp;<span class="hab-line">' + (hab.EsExFumador ? 'Sí' : '') + '</span></div>';
        habHtml += '<div class="hab-row"><span class="cb' + (hab.UsaDrogas ? ' filled' : '') + '"></span><span style="width:50px;">Drogas:</span>';
        habHtml += '<strong>Tipo de droga:</strong>&nbsp;<span class="hab-line">' + escapeHtml(trunc(toText(hab.TipoDrogas), 25)) + '</span></div>';
        habHtml += '<div class="hab-row"><span class="cb' + (hab.BebeAlcohol ? ' filled' : '') + '"></span><span>Alcohol:</span>';
        habHtml += '<span class="hab-line" style="margin-left:10px;">' + escapeHtml(trunc(toText(hab.FrecuenciaAlcohol), 20)) + '</span></div>';
        habHtml += '<div class="hab-row"><span class="cb' + (hab.HaceDeporte ? ' filled' : '') + '"></span><span style="width:50px;">Deporte:</span>';
        habHtml += '<span class="hab-line">' + escapeHtml(trunc(toText(hab.TipoDeporte), 20)) + '</span>';
        habHtml += '&nbsp;<strong>Frecuencia</strong>&nbsp;<span class="hab-line">' + escapeHtml(trunc(toText(hab.FrecuenciaDeporte), 15)) + '</span></div>';
        habHtml += '<div class="hab-row"><span style="width:85px;"><strong>Tiempo Libre:</strong></span>';
        habHtml += '<span class="hab-line">' + escapeHtml(trunc(toText(hab.DescripcionTiempoLibre), 35)) + '</span></div>';
        $('#prevHabitosContainer').html(habHtml);
    });

    safeSection('vacunas', function () {
        var vac = ev.Vacunacion || {};
        var vacHtml = '<strong>VACUNAS</strong>&nbsp;&nbsp;';
        vacHtml += '<strong>TÉTANOS</strong>&nbsp;1<span class="vac-line">' + (vac.TetanosDosis1 ? 'X' : '') + '</span>';
        vacHtml += '&nbsp;2<span class="vac-line">' + (vac.TetanosDosis2 ? 'X' : '') + '</span>';
        vacHtml += '&nbsp;3<span class="vac-line">' + (vac.TetanosDosis3 ? 'X' : '') + '</span>';
        vacHtml += '&nbsp;&nbsp;<strong>Hepatitis</strong>&nbsp;1<span class="vac-line">' + (vac.HepatitisDosis1 ? 'X' : '') + '</span>';
        vacHtml += '&nbsp;2<span class="vac-line">' + (vac.HepatitisDosis2 ? 'X' : '') + '</span>';
        vacHtml += '&nbsp;&nbsp;<strong>H1N1:</strong><span class="vac-line">' + (vac.InfluenzaH1N1 ? 'X' : '') + '</span>';
        $('#prevVacunasContainer').html(vacHtml);
    });

    safeSection('exploracion-signos', function () {
        $('#prevTa').text((ev.PresionSistolica && ev.PresionDiastolica) ? (ev.PresionSistolica + '/' + ev.PresionDiastolica) : '');
        $('#prevFc').text(toText(ev.FrecuenciaCardiaca));
        $('#prevFr').text(toText(ev.FrecuenciaRespiratoria));
        $('#prevTemp').text(toText(ev.Temperatura));
        $('#prevPeso').text(toText(ev.PesoKg));
        $('#prevEstatura').text(toText(ev.AlturaMetros));
        $('#prevImc').text(toText(ev.Imc));
        $('#prevGlucosa').text(toText(ev.Glucosa));
        $('#prevOximetria').text(toText(ev.Oximetria));
        $('#prevImcDesc').text(toText(ev.ImcDescripcion));
        $('#prevAlergias').text(trunc(toText(ev.Alergias), 60) || 'No registradas');
        $('#prevAparatos').text(trunc(toText(ev.AparatosSistemas), 50));
        $('#prevSintomas').text(trunc(toText(ev.SintomasPaciente), 100));
    });

    // Tabla de exploración (20 items)
    var areas = [
        'Cabeza','Ojos','Nariz','Boca','Dentadura','Faringe','Amígdalas','Otoscopia','Cuello',
        'Columna-espalda','Extremidades','Piel','Ap. Respiratorio','Cardiaco','Vascular periférico',
        'Abdomen','Neurológico','Genitales','Hernias','Otro'
    ];
    safeSection('exploracion-tabla', function () {
        var efMap = {};
        if(ev.OrdenExamenFisico && ev.OrdenExamenFisico.length){
            ev.OrdenExamenFisico.forEach(function(x){
                if(x && x.SistemaCuerpo) efMap[normalizeKey(x.SistemaCuerpo)] = x;
            });
        }
        var explHtml = '<tr><th class="col-item"></th><th class="col-norm">Normal</th><th class="col-anorm">Anormal</th><th class="col-desc">Descripción de Hallazgos</th></tr>';
        areas.forEach(function(a, idx){
            var found = efMap[normalizeKey(a)] || null;
            var normal = found ? !!found.EsNormal : false;
            var anormal = found ? !found.EsNormal && found.Hallazgos : false;
            var hall = found ? trunc(toText(found.Hallazgos), 30) : '';
            explHtml += '<tr><td>' + (idx+1) + '. ' + a + ':</td>';
            explHtml += '<td class="col-norm">' + (normal ? 'X' : '') + '</td>';
            explHtml += '<td class="col-anorm">' + (anormal ? 'X' : '') + '</td>';
            explHtml += '<td>' + escapeHtml(hall) + '</td></tr>';
        });
        $('#prevExploracion').html(explHtml);
    });

    safeSection('sexo-especifico', function () {
        if(ev.DetalleFemenino){
            var f2 = ev.DetalleFemenino;
            $('#prevGinecoBlock').show();
            $('#prevMasculinoBlock').hide();
            $('#prevPlanificacion').text(trunc(toText(f2.MetodoPlanificacion), 15));
            $('#prevMenarca').text(toText(f2.EdadMenarca));
            $('#prevCiclos').text(trunc(toText(f2.Ciclos), 10));
            $('#prevFum').text(formatDate(f2.FechaUltimaMenstruacion) || '');
            $('#prevNumHijos').text(trunc(toText(f2.NumeroHijosEdades), 15));
            $('#prevIvsaFem').text(toText(f2.Ivsa));
            $('#prevCitVag').text(formatDate(f2.FechaUltimoPapanicolau) || '');
            $('#prevEts').text(trunc(toText(f2.Ets), 10));
            $('#prevGestas').text(toText(f2.Gestas));
            $('#prevPartos').text(toText(f2.Partos));
            $('#prevAbortos').text(toText(f2.Abortos));
            $('#prevCesareas').text(toText(f2.Cesareas));
        } else if(ev.DetalleMasculino) {
            $('#prevGinecoBlock').hide();
            $('#prevMasculinoBlock').show();
            var m2 = ev.DetalleMasculino;
            $('#prevPrepucio').toggleClass('filled', !!m2.PrepucioRetractil);
            $('#prevTesticulos').toggleClass('filled', !!m2.TesticulosDescendidos);
            $('#prevFimosis').toggleClass('filled', !!m2.Fimosis);
            $('#prevCriptorquidia').toggleClass('filled', !!m2.Criptorquidia);
            $('#prevVaricocele').toggleClass('filled', !!m2.Varicocele);
            $('#prevHidrocele').toggleClass('filled', !!m2.Hidrocele);
            $('#prevHernia').toggleClass('filled', !!m2.Hernia);
            $('#prevIvsaMasc').toggleClass('filled', !!m2.Ivsa);
            $('#prevPsa').toggleClass('filled', !!m2.Psa);
            $('#prevMpf').toggleClass('filled', !!m2.MetodoPlanificacion);
        } else {
            $('#prevGinecoBlock').hide();
            $('#prevMasculinoBlock').hide();
        }
    });

    safeSection('agudeza-visual', function () {
        var av = ev.AgudezaVisual || {};
        $('#prevOdSin').text(toText(av.OdSinLentes));
        $('#prevOiSin').text(toText(av.OiSinLentes));
        $('#prevAoSin').text(toText(av.AoSinLentes));
        $('#prevOdCon').text(toText(av.OdConLentes));
        $('#prevOiCon').text(toText(av.OiConLentes));
        $('#prevAoCon').text(toText(av.AoConLentes));
        $('#prevUsaLentes').text(toText(av.UsaLentes));
        $('#prevDaltonismo').text(toText(av.Daltonismo));
        $('#prevRefVisual').text(toText(av.ReferenciaVisual));
    });

    safeSection('columna-resultado', function () {
        var col = ev.Columna || {};
        function cvVal(v){
            var n = toNumber(v);
            return n === 1 ? 'N' : (n === 2 ? 'A' : (n === 3 ? 'D' : ''));
        }
        $('#prevLordC').text(cvVal(col.LordosisCervical));
        $('#prevLordD').text(cvVal(col.LordosisDorsal));
        $('#prevLordL').text(cvVal(col.LordosisLumbar));
        $('#prevCifoC').text(cvVal(col.CifosisCervical));
        $('#prevCifoD').text(cvVal(col.CifosisDorsal));
        $('#prevCifoL').text(cvVal(col.CifosisLumbar));
        $('#prevEscDd').text(col.EscoliosisDorsalDerecha ? 'X' : '');
        $('#prevEscLd').text(col.EscoliosisLumbarDerecha ? 'X' : '');
        $('#prevEscDobD').text(col.EscoliosisDobleDerecha ? 'X' : '');
        $('#prevEscDi').text(col.EscoliosisDorsalIzquierda ? 'X' : '');
        $('#prevEscLi').text(col.EscoliosisLumbarIzquierda ? 'X' : '');
        $('#prevEscDobI').text(col.EscoliosisDobleIzquierda ? 'X' : '');

        $('#prevDiagnostico').text(trunc(toText(ev.Observaciones), 80));
        $('#prevDiagnostico2').text('');

        var apt = toNumber(ev.FkAptitudMedica);
        $('#prevResApto').toggleClass('filled', apt === 1);
        $('#prevResNoApto').toggleClass('filled', apt === 3);
        $('#prevResRestr').toggleClass('filled', apt === 2);

        $('#prevRecomendaciones').text(trunc(toText(ev.Recomendaciones), 80));
        $('#prevRecomendaciones2').text('');
    });
}

function getTipoSangre(fk){
    var map = {1:'O+',2:'O-',3:'A+',4:'A-',5:'B+',6:'B-',7:'AB+',8:'AB-'};
    return map[fk] || '';
}


function setBox(id, on){
    var $b = $('#' + id);
    if(!$b.length) return;
    $b.toggleClass('on', !!on);
}

function setOptLine(id, on){
    var $e = $('#' + id);
    if(!$e.length) return;
    $e.text(on ? 'X' : '');
}

function mkMale(lbl, val){
    return '<div class="m-item"><span class="box' + (val ? ' on' : '') + '"></span><span>' + lbl + '</span></div>';
}

function mapCurva(v){
    // N: Normal / A: Aumentada / D: Disminuida (el VM usa int?)
    if(v === null || v === undefined) return '';
    if(v === 'N' || v === 'A' || v === 'D') return v;
    var n = parseInt(v,10);
    if(n === 1) return 'N';
    if(n === 2) return 'A';
    if(n === 3) return 'D';
    return '';
}

function safeVal(v){
    if(v === null || v === undefined) return '';
    return (''+v).toUpperCase();
}

function formatDate(d){
    if(!d) return '';
    // Soporta /Date( )/ o ISO
    if(typeof d === 'string'){
        var m = d.match(/\/Date\((\d+)\)\//);
        if(m) {
            var dt = new Date(parseInt(m[1],10));
            return pad2(dt.getDate()) + '/' + pad2(dt.getMonth()+1) + '/' + dt.getFullYear();
        }
        if(/^\d{4}-\d{2}-\d{2}/.test(d)){
            return d.substring(8,10) + '/' + d.substring(5,7) + '/' + d.substring(0,4);
        }
        return d;
    }
    try{
        var dt2 = new Date(d);
        if(isNaN(dt2.getTime())) return '';
        return pad2(dt2.getDate()) + '/' + pad2(dt2.getMonth()+1) + '/' + dt2.getFullYear();
    }catch(e){ return ''; }
}

function pad2(n){ return (n < 10 ? '0' : '') + n; }

function escapeHtml(s){
    if(s === null || s === undefined) return '';
    return (''+s)
        .replace(/&/g,'&amp;')
        .replace(/</g,'&lt;')
        .replace(/>/g,'&gt;')
        .replace(/"/g,'&quot;')
        .replace(/\'/g,'&#39;');
}

function cargarSolicitudes() { aplicarFiltros(); }

function aplicarFiltros(resetPage) {
    if (resetPage) paginaActual = 1;
    
    var estatusVal = $('#filtroEstatus').val();
    var filtroEstatus = (estatusVal && estatusVal !== "-1") ? parseInt(estatusVal) : null;
    var soloPendientes = (estatusVal === "-1");

    var req = {
        pagina: paginaActual,
        tamanoPagina: registrosPorPagina,
        filtroNumEmpleado: $('#filtroNumEmp').val() ? parseInt($('#filtroNumEmp').val()) : null,
        filtroModalidad: $('#filtroModalidad').val() || null,
        filtroEstatus: soloPendientes ? -1 : filtroEstatus,
        fechaDesde: $('#filtroFechaDesde').val() || null,
        fechaHasta: null,
        filtroEmpresa: null,
        filtroArea: null,
        filtroAnio: null,
        filtroSemana: null
    };

    $('#tbodySolicitudes').html('<tr><td colspan="10" class="no-data">Cargando bandeja médica...</td></tr>');

    apiCall({
        url: 'DashboardServicioMedico.aspx/CargarPagina',
        data: req,
        onSuccess: function (r) {
            var resp = r.d;
            if (resp.success) {
                totalRegistrosGlobal = resp.total;
                renderPagina(resp.data);
            } else {
                renderTableError(resp.message || 'No fue posible cargar la bandeja.');
            }
        },
        onError: function (xhr, status, err, msg) {
            renderTableError(msg);
        }
    });
}

function renderPagina(datos) {
    var $tbody = $('#tbodySolicitudes');
    $tbody.empty();

    if (!datos || datos.length === 0) {
        $tbody.append('<tr><td colspan="10" class="no-data">No hay solicitudes pendientes en este momento.</td></tr>');
        renderPaginacion();
        return;
    }

    datos.forEach(function (s) {
        var estLow = (s.EstatusDesc || '').toLowerCase();
        var badgeEst = 'badge-pendiente';
        if (estLow.indexOf('proceso') >= 0) badgeEst = 'badge-proceso';
        if (estLow.indexOf('complet') >= 0) badgeEst = 'badge-completado';

        // Modalidad badge (Text only)
        var modClass = (s.Modalidad === 'INGRESO' ? 'badge-ingreso' : 'badge-periodico');

        // Indicadores de exámenes completados (Solo texto coloreado)
        var examIndicators = '';
        if (s.TieneEvaluacion) {
            examIndicators += '<div class="status-badge badge-med" title="Evaluación Médica Completada"><i class="fas fa-heartbeat"></i> MÉDICO OK</div>';
        }
        if (s.TieneAntidoping) {
            examIndicators += '<div class="status-badge badge-tox" title="Antidoping Completado"><i class="fas fa-vial"></i> TOX OK</div>';
        }

        // Aptitud badge (solo si está completada)
        var aptitudHtml = '';
        if (estLow.indexOf('complet') >= 0) {
            aptitudHtml = '<span class="status-badge badge-apto"><i class="fas fa-check-double"></i> APTO</span>';
        } else {
            aptitudHtml = '<span style="color:#ccc;">-</span>';
        }

        var row = '<tr data-pk="' + s.PkOrdenMedico + '" onclick="verDetalle(' + s.PkOrdenMedico + ')">' +
            '<td><strong>' + s.FolioDisplay + '</strong></td>' +
            '<td>' + (s.FechaOrdenFormateada || '-') + '</td>' +
            '<td><span class="status-badge ' + modClass + '">' + (s.Modalidad || 'INGRESO') + '</span></td>' +
            '<td><strong>' + (s.NombrePersona || 'SIN NOMBRE') + '</strong></td>' +
            '<td>' + (s.EmpresaNombre || '-') + '</td>' +
            '<td>' + (s.ProyectoDesc || '-') + '</td>' +
            '<td>' + (s.TipoServicioDesc || '-') + '</td>' +
            '<td>' +
                '<span class="status-badge ' + badgeEst + '">' + (s.EstatusDesc || 'Pendiente') + '</span>' +
                '<div style="margin-top:4px; display:flex; flex-direction:column; gap:2px;">' + examIndicators + '</div>' +
            '</td>' +
            '<td style="text-align:center;">' + aptitudHtml + '</td>' +
            '<td style="text-align:center;">' +
               '<div style="display:flex; justify-content:center; gap:5px;">' +
                   '<button type="button" class="btn-action" title="Ver Detalle" onclick="event.stopPropagation(); verDetalle(' + s.PkOrdenMedico + ')"><i class="fas fa-eye"></i></button>' +
                   '<button type="button" class="btn-action" title="Ver Evaluación" onclick="event.stopPropagation(); verEvaluacionPreview(' + s.PkOrdenMedico + ')"><i class="fas fa-file-medical"></i></button>' +
                   '<button type="button" class="btn-action" title="Ver Pase" onclick="event.stopPropagation(); verPaseEmpleado(' + s.PkOrdenMedico + ')"><i class="fas fa-file-signature"></i></button>' +
               '</div>' +
            '</td>' +
            '</tr>';
        $tbody.append(row);
    });

    renderPaginacion();
}

function renderPaginacion() {
    var totalPaginas = Math.ceil(totalRegistrosGlobal / registrosPorPagina) || 1;
    var $cont = $('#paginacionControles');
    $cont.empty();

    if (totalPaginas <= 1) return;

    // Lógica simplificada de paginación
    $cont.append('<button onclick="cambiarPagina(' + (paginaActual - 1) + ')" ' + (paginaActual === 1 ? 'disabled' : '') + '>Ant.</button>');
    $cont.append('<span>Página ' + paginaActual + ' de ' + totalPaginas + '</span>');
    $cont.append('<button onclick="cambiarPagina(' + (paginaActual + 1) + ')" ' + (paginaActual === totalPaginas ? 'disabled' : '') + '>Sig.</button>');
}

function cambiarPagina(p) {
    if (p < 1) return;
    paginaActual = p;
    aplicarFiltros();
}

function onCambioTamanoPagina() {
    registrosPorPagina = parseInt($('#selectTamanoPagina').val()) || 25;
    aplicarFiltros(true);
}

function verDetalle(id) {
    // Construir endpoint sobre el path actual para evitar problemas de rutas/base href/virtual dirs.
    var endpoint = window.location.pathname + '/VerDetalle';

    apiCall({
        url: endpoint,
        data: { id: id },
        onSuccess: function (r) {
            var resp = r.d;
            if (resp && resp.success) {
                var o = resp.orden;
                $('#modalFolio').text(o.FolioDisplay);
                $('#detGeneralEmpresa').text(o.EmpresaNombre);
                $('#detGeneralProyecto').text(o.ProyectoDesc);
                $('#detModalidad').text(o.Modalidad);
                $('#detTipoServicio').text(o.TipoServicioDesc);
                $('#detEstatus').text(o.EstatusDesc);

                $('#detPacienteNombre').text(o.NombrePersona);
                
                if (o.Modalidad === 'INGRESO') {
                    $('#seccionIngresoNotice').show();
                    $('#lblNomPersona').text('NOMBRE DEL CANDIDATO');
                    $('#detPacienteEmpNum').text('-');
                    $('#detPacientePuesto').text(o.PuestoCandidato || '-');
                    $('#detPacienteNss').text(o.NssCandidato || '-');
                } else {
                    $('#seccionIngresoNotice').hide();
                    $('#lblNomPersona').text('NOMBRE DEL EMPLEADO');
                    var emp = resp.empleado || {};
                    $('#detPacienteEmpNum').text(emp.PkEmpleado || o.FkEmpleado || '-');
                    $('#detPacientePuesto').text(emp.PuestoDesc || o.PuestoCandidato || '-');
                    $('#detPacienteNss').text(emp.Nss || o.NssCandidato || '-');
                }

                // Acciones específicas para el Médico
                var wizardUrl = 'EvaluacionMedica.aspx?id=' + o.PkOrdenMedico;
                $('#btnIrEvaluar').attr('href', wizardUrl).toggle(o.FkEstatus === 1); // Solo si está pendiente
                
                // Botón Resultados si ya está completada
                if (o.FkEstatus === 3) {
                     $('#btnVerResultados').attr('href', '#').off('click').on('click', function(e){
                         e.preventDefault();
                         verEvaluacionPreview(o.PkOrdenMedico);
                     }).show();
                } else {
                     $('#btnVerResultados').hide();
                }

                $('#modalDetalle').addClass('active');
            } else {
                alert((resp && resp.message) ? resp.message : 'No fue posible cargar el detalle.');
            }
        },
        onError: function (xhr, status, err, msg) {
            alert(msg);
        }
    });
}

function cerrarModal() { $('.modal-overlay').removeClass('active'); }

function limpiarFiltros() {
    $('#filtroNumEmp').val('');
    $('#filtroModalidad').val('');
    $('#filtroEstatus').val('-1');
    $('#filtroFechaDesde').val('');
    aplicarFiltros(true);
}

// ==================== MODAL EVALUACION EMPLEADO ====================
var empleadoSeleccionado = null;

function abrirModalEvaluacionEmpleado() {
    $('#modalEvaluacionEmpleado').show();
    $('#txtNumEmpleado').val('').focus();
    $('#empResultado').html('');
    $('#btnCrearEvaluacion').hide();
    empleadoSeleccionado = null;
}

function cerrarModalEvaluacionEmpleado() {
    $('#modalEvaluacionEmpleado').hide();
    $('#txtNumEmpleado').val('');
    $('#empResultado').html('');
    $('#btnCrearEvaluacion').hide();
    empleadoSeleccionado = null;
}

function buscarEmpleado() {
    var numEmp = $('#txtNumEmpleado').val();
    if (!numEmp || numEmp <= 0) {
        $('#empResultado').html('<div class="emp-not-found"><i class="fas fa-exclamation-circle"></i> Ingrese un número de empleado válido</div>');
        $('#btnCrearEvaluacion').hide();
        return;
    }

    $('#empResultado').html('<div style="text-align:center; padding:20px;"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>');

    apiCall({
        url: 'DashboardServicioMedico.aspx/BuscarEmpleado',
        data: { numeroEmpleado: parseInt(numEmp) },
        onSuccess: function (r) {
            var resp = r.d;
            if (resp.success && resp.empleado) {
                empleadoSeleccionado = resp.empleado;
                mostrarDatosEmpleado(resp.empleado);
                $('#btnCrearEvaluacion').show();
            } else {
                $('#empResultado').html('<div class="emp-not-found"><i class="fas fa-user-slash"></i> ' + (resp.message || 'Empleado no encontrado') + '</div>');
                $('#btnCrearEvaluacion').hide();
                empleadoSeleccionado = null;
            }
        },
        onError: function (xhr, status, err, msg) {
            $('#empResultado').html('<div class="emp-not-found"><i class="fas fa-exclamation-triangle"></i> ' + msg + '</div>');
            $('#btnCrearEvaluacion').hide();
            empleadoSeleccionado = null;
        }
    });
}

function mostrarDatosEmpleado(emp) {
    var html = '<div class="emp-data-card">';
    html += '<div class="emp-data-row"><div class="emp-data-label">Número:</div><div class="emp-data-value">' + escapeHtml(emp.NumeroEmpleado || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">Nombre:</div><div class="emp-data-value">' + escapeHtml(emp.NombreCompleto || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">CURP:</div><div class="emp-data-value">' + escapeHtml(emp.Curp || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">NSS:</div><div class="emp-data-value">' + escapeHtml(emp.Nss || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">Puesto:</div><div class="emp-data-value">' + escapeHtml(emp.Puesto || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">Empresa:</div><div class="emp-data-value">' + escapeHtml(emp.Empresa || '') + '</div></div>';
    
    // Mostrar si tiene evaluación previa
    if (emp.DatosPrevios) {
        html += '<div style="margin-top:12px; padding-top:10px; border-top:1px dashed #aaa;">';
        html += '<strong style="color:#28a745;"><i class="fas fa-history"></i> Se encontró evaluación previa - los datos se heredarán</strong>';
        html += '</div>';
    }
    
    html += '</div>';
    $('#empResultado').html(html);
}

function crearEvaluacionEmpleado() {
    if (!empleadoSeleccionado) {
        alert('Primero busque y seleccione un empleado');
        return;
    }

    if (!empleadoSeleccionado.PkOrdenMedico) {
        alert('No se pudo crear la orden de evaluación');
        return;
    }

    // Cerrar modal y mostrar el pase generado
    cerrarModalEvaluacionEmpleado();
    verPaseEmpleado(empleadoSeleccionado.PkOrdenMedico);
}

window.verPaseEmpleado = function(pkOrdenMedico) {
    mostrarModalPase(pkOrdenMedico);
};

window.mostrarModalPase = function(pkOrdenMedico) {
    $('#paseContent').html('<div style="text-align:center; padding:40px;"><div class="gci-spinner"></div><p>Cargando pase...</p></div>');
    $('#modalPase').addClass('active');

    apiCall({
        url: 'DashboardServicioMedico.aspx/ObtenerPaseHtml',
        data: { pkOrdenMedico: pkOrdenMedico },
        timeout: 15000,
        onSuccess: function(r) {
            if (r.d && r.d.success) {
                $('#paseContent').html(r.d.html);
            } else {
                var msg = (r.d && r.d.message) ? r.d.message : 'Error al cargar el pase';
                $('#paseContent').html('<div style="text-align:center; padding:40px; color:#d32f2f;"><i class="fas fa-exclamation-circle" style="font-size:48px; margin-bottom:15px;"></i><p>' + msg + '</p><button type="button" class="btn-gci btn-gci-primary" onclick="mostrarModalPase(' + pkOrdenMedico + ')" style="margin-top:15px;"><i class="fas fa-sync"></i> Reintentar</button></div>');
            }
        },
        onError: function(xhr, status, err, msg) {
            $('#paseContent').html('<div style="text-align:center; padding:40px; color:#d32f2f;"><i class="fas fa-exclamation-circle" style="font-size:48px; margin-bottom:15px;"></i><p>' + msg + '</p><button type="button" class="btn-gci btn-gci-primary" onclick="mostrarModalPase(' + pkOrdenMedico + ')" style="margin-top:15px;"><i class="fas fa-sync"></i> Reintentar</button></div>');
        }
    });
};

window.cerrarModalPase = function() {
    $('#modalPase').removeClass('active');
    $('#paseContent').html('');
};

window.imprimirPaseModal = function() {
    var contenido = document.getElementById('paseContent').innerHTML;
    var ventana = window.open('', '_blank', 'width=900,height=700,scrollbars=yes');
    if (ventana) {
        ventana.document.write('<!DOCTYPE html><html><head><title>Pase Médico</title></head><body>' + contenido + '</body></html>');
        ventana.document.close();
        ventana.focus();
        setTimeout(function() { ventana.print(); }, 500);
    } else {
        alert('Permita ventanas emergentes para imprimir');
    }
};

// Permitir buscar con Enter en el campo de número de empleado
$(document).on('keypress', '#txtNumEmpleado', function(e) {
    if (e.which === 13) {
        buscarEmpleado();
    }
});
