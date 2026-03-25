/* ══════════════════════════════════════════════════════════════════════════
   CONFIG & STATE
══════════════════════════════════════════════════════════════════════════ */
const API   = '/api/markers';
const COLORS = ['red','blue','green','yellow','purple','orange','pink','cyan','white','black'];
const PIN_COLORS = {
  red:'#e53935', blue:'#1a73e8', green:'#2e7d32', yellow:'#f9a825',
  purple:'#6a1b9a', orange:'#e65100', pink:'#e91e63', cyan:'#00838f',
  white:'#eeeeee', black:'#212121'
};

let map, markers = {}, editingId = null, tempMarker = null;

/* ══════════════════════════════════════════════════════════════════════════
   GOOGLE MAPS INIT  (called by the Maps SDK callback)
══════════════════════════════════════════════════════════════════════════ */
async function initMap() {
  const { Map } = await google.maps.importLibrary('maps');
  const { AdvancedMarkerElement, PinElement } = await google.maps.importLibrary('marker');

  window._MapLib    = { Map };
  window._MarkerLib = { AdvancedMarkerElement, PinElement };

  map = new Map(document.getElementById('map'), {
    center:  { lat: 25, lng: 15 },
    zoom:    3,
    mapId:   'markers_demo',
    disableDefaultUI: false,
    streetViewControl: false,
    mapTypeControl: false,
    fullscreenControl: true,
  });

  // Click on map → populate coordinate fields
  map.addListener('click', (e) => {
    const lat = e.latLng.lat().toFixed(6);
    const lng = e.latLng.lng().toFixed(6);
    $('#lat').val(lat);
    $('#lng').val(lng);

    // Show temp draggable pin
    if (tempMarker) tempMarker.map = null;
    const pin = new PinElement({ background: '#555', borderColor: '#333', glyphColor: '#fff' });
    tempMarker = new AdvancedMarkerElement({
      map, position: e.latLng, content: pin.element,
      title: 'New marker position (not saved yet)', gmpDraggable: true
    });
    tempMarker.addListener('dragend', (ev) => {
      $('#lat').val(ev.latLng.lat().toFixed(6));
      $('#lng').val(ev.latLng.lng().toFixed(6));
    });
    showToast('Coordinates filled — complete the form and click Add Marker');
  });

  buildColorPicker();
  loadMarkers();
}

/* ══════════════════════════════════════════════════════════════════════════
   COLOR PICKER
══════════════════════════════════════════════════════════════════════════ */
function buildColorPicker() {
  const $cp = $('#color-picker').empty();
  COLORS.forEach(c => {
    $('<div>')
      .addClass('color-dot' + (c === 'red' ? ' active' : ''))
      .css('background', PIN_COLORS[c])
      .attr('data-color', c)
      .on('click', function() {
        $('.color-dot').removeClass('active');
        $(this).addClass('active');
        $('#selected-color').val(c);
      })
      .appendTo($cp);
  });
}

/* ══════════════════════════════════════════════════════════════════════════
   AJAX — LOAD ALL MARKERS
══════════════════════════════════════════════════════════════════════════ */
function loadMarkers() {
  $.ajax({
    url: API,
    method: 'GET',
    success(data) {
      // Clear existing
      Object.values(markers).forEach(m => m.map = null);
      markers = {};
      $('#marker-list').empty();

      data.forEach(addMarkerToUI);
      updateBadge();
      $('#loading').fadeOut(300);
    },
    error(xhr) {
      $('#loading').fadeOut();
      showToast('Failed to load markers: ' + (xhr.responseJSON?.error || xhr.statusText), 'error');
    }
  });
}

/* ══════════════════════════════════════════════════════════════════════════
   ADD MARKER TO MAP + SIDEBAR
══════════════════════════════════════════════════════════════════════════ */
function addMarkerToUI(data) {
  const { AdvancedMarkerElement, PinElement } = window._MarkerLib;
  const color = data.color || 'red';
  const bg    = PIN_COLORS[color] || '#e53935';

  const pin = new PinElement({ background: bg, borderColor: shadeColor(bg, -30), glyphColor: '#fff' });
  const m   = new AdvancedMarkerElement({
    map,
    position: { lat: data.latitude, lng: data.longitude },
    content:  pin.element,
    title:    data.title,
  });

  m.addListener('click', () => panToMarker(data.id));
  markers[data.id] = m;

  // Sidebar row
  const $row = $('<div class="marker-item">')
    .attr('data-id', data.id)
    .html(`
      <div class="marker-dot" style="background:${bg}"></div>
      <div class="marker-info">
        <strong>${escHtml(data.title)}</strong>
        <span>${data.latitude.toFixed(4)}, ${data.longitude.toFixed(4)}</span>
      </div>
      <div class="marker-actions">
        <button class="btn btn-ghost btn-sm edit-btn" data-id="${data.id}" title="Edit">✏️</button>
        <button class="btn btn-danger btn-sm delete-btn" data-id="${data.id}" title="Delete">🗑️</button>
      </div>
    `)
    .on('click', function(e) {
      if ($(e.target).closest('button').length) return; // handled below
      panToMarker(data.id);
    });

  $('#marker-list').prepend($row);
}

/* ══════════════════════════════════════════════════════════════════════════
   SAVE (CREATE or UPDATE)
══════════════════════════════════════════════════════════════════════════ */
$('#save-btn').on('click', function() {
  const title = $('#title').val().trim();
  const lat   = parseFloat($('#lat').val());
  const lng   = parseFloat($('#lng').val());

  if (!title)            return showToast('Title is required.', 'error');
  if (isNaN(lat)||isNaN(lng)) return showToast('Valid coordinates are required.', 'error');
  if (lat < -90  || lat > 90)  return showToast('Latitude must be between −90 and 90.', 'error');
  if (lng < -180 || lng > 180) return showToast('Longitude must be between −180 and 180.', 'error');

  const payload = {
    title,
    description: $('#description').val().trim(),
    latitude:    lat,
    longitude:   lng,
    color:       $('#selected-color').val()
  };

  const isEdit  = editingId !== null;
  const url     = isEdit ? `${API}/${editingId}` : API;
  const method  = isEdit ? 'PUT' : 'POST';

  const $saveBtn = $('#save-btn');
  const $saveLabel = $('#save-label');
  $saveBtn.prop('disabled', true);
  $saveLabel.text('Saving…');

  $.ajax({
    url, method,
    contentType: 'application/json',
    data: JSON.stringify(payload),
    success(data) {
      if (isEdit) {
        // Remove old map pin + sidebar row
        if (markers[editingId]) markers[editingId].map = null;
        delete markers[editingId];
        $(`[data-id="${editingId}"]`).remove();
        cancelEdit();
      }
      addMarkerToUI(data);
      updateBadge();
      panToMarker(data.id);
      clearForm();
      if (tempMarker) { tempMarker.map = null; tempMarker = null; }
      showToast(isEdit ? 'Marker updated!' : 'Marker saved!', 'success');
    },
    error(xhr) {
      showToast('Save failed: ' + (xhr.responseJSON?.error || xhr.statusText), 'error');
    },
    complete() {
      $saveBtn.prop('disabled', false);
      $saveLabel.text(editingId ? 'Update Marker' : 'Add Marker');
    }
  });
});

/* ══════════════════════════════════════════════════════════════════════════
   EDIT
══════════════════════════════════════════════════════════════════════════ */
$(document).on('click', '.edit-btn', function(e) {
  e.stopPropagation();
  const id = parseInt($(this).data('id'));

  $.ajax({
    url: `${API}/${id}`,
    method: 'GET',
    success(data) {
      editingId = data.id;
      $('#title').val(data.title);
      $('#description').val(data.description);
      $('#lat').val(data.latitude);
      $('#lng').val(data.longitude);

      // Set color
      $('.color-dot').removeClass('active');
      $(`.color-dot[data-color="${data.color}"]`).addClass('active');
      $('#selected-color').val(data.color);

      $('#form-heading').text('Edit Marker');
      $('#save-label').text('Update Marker');
      $('#edit-banner').addClass('show');

      $('[data-id]').removeClass('active');
      $(`[data-id="${id}"]`).addClass('active');

      window.scrollTo(0, 0);
    },
    error() { showToast('Could not load marker for editing.', 'error'); }
  });
});

$('#cancel-edit').on('click', cancelEdit);

function cancelEdit() {
  editingId = null;
  clearForm();
  $('#form-heading').text('Add New Marker');
  $('#save-label').text('Add Marker');
  $('#edit-banner').removeClass('show');
  $('[data-id]').removeClass('active');
}

/* ══════════════════════════════════════════════════════════════════════════
   DELETE
══════════════════════════════════════════════════════════════════════════ */
$(document).on('click', '.delete-btn', function(e) {
  e.stopPropagation();
  const id = parseInt($(this).data('id'));
  const name = $(this).closest('.marker-item').find('strong').text();

  if (!confirm(`Delete "${name}"?`)) return;

  $.ajax({
    url: `${API}/${id}`,
    method: 'DELETE',
    success() {
      if (markers[id]) markers[id].map = null;
      delete markers[id];
      $(`[data-id="${id}"]`).fadeOut(200, function(){ $(this).remove(); });
      updateBadge();
      if (editingId === id) cancelEdit();
      showToast('Marker deleted.', 'success');
    },
    error(xhr) {
      showToast('Delete failed: ' + (xhr.responseJSON?.error || xhr.statusText), 'error');
    }
  });
});

/* ══════════════════════════════════════════════════════════════════════════
   HELPERS
══════════════════════════════════════════════════════════════════════════ */
function panToMarker(id) {
  const m = markers[id];
  if (!m) return;
  map.panTo(m.position);
  map.setZoom(10);
  $('[data-id]').removeClass('active');
  $(`[data-id="${id}"]`).addClass('active');
}

function clearForm() {
  $('#title, #description, #lat, #lng').val('');
  $('.color-dot').removeClass('active');
  $('.color-dot[data-color="red"]').addClass('active');
  $('#selected-color').val('red');
}

function updateBadge() {
  const n = Object.keys(markers).length;
  $('#count-badge').text(n + (n === 1 ? ' marker' : ' markers'));
}

let toastTimer;
function showToast(msg, type = '') {
  clearTimeout(toastTimer);
  $('#toast').text(msg).removeClass('success error').addClass(type + ' show');
  toastTimer = setTimeout(() => $('#toast').removeClass('show'), 3000);
}

function escHtml(str) {
  return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function shadeColor(hex, pct) {
  const num = parseInt(hex.replace('#',''), 16);
  const r = Math.min(255, Math.max(0, (num >> 16) + pct));
  const g = Math.min(255, Math.max(0, ((num >> 8) & 0xff) + pct));
  const b = Math.min(255, Math.max(0, (num & 0xff) + pct));
  return '#' + [r,g,b].map(v => v.toString(16).padStart(2,'0')).join('');
}

/* ══════════════════════════════════════════════════════════════════════════
   BOOTSTRAP MAPS SDK — fetch API key from backend, then load SDK
══════════════════════════════════════════════════════════════════════════ */
$.get('/api/markers/apikey', function(res) {
  if (!res.key) {
    $('#loading').html('<span style="color:red">⚠️ Google Maps API key not configured in appsettings.json</span>');
    return;
  }
  const script = document.createElement('script');
  script.src = `https://maps.googleapis.com/maps/api/js?key=${res.key}&loading=async&callback=initMap&libraries=maps,marker`;
  script.async = true;
  script.defer = true;
  document.head.appendChild(script);
});
