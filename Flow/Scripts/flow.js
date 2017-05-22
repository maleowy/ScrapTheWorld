
var $flowchart = $('#chart');
var $container = $flowchart.parent();
	
var $operatorProperties = $('#operator_properties');
var $linkProperties = $('#link_properties');
var $operatorTitle = $('#operator_title');
var $operatorData = $('#operator_data');
var $linkColor = $('#link_color');
	
var $events = $('#events');
var $eventsContainer = $('#events_container');	
	
var operatorI = 0;	
var data = {};
	
	
function add() {		
    operatorI++;
    var operatorId = 'created_operator_' + operatorI;
		
    var operatorData = {
        top: 60,
        left: 500,
        properties: {
            title: 'Node ' + operatorI,
            commands: '--command',
            inputs: {
                input_1: {
                    label: 'I',
                }
            },
            outputs: {
                output_1: {
                    label: 'O',
                }
            }
        }
    };
      
    $flowchart.flowchart('createOperator', operatorId, operatorData);
}

function deleteSelected() {
	$flowchart.flowchart('deleteSelected');
}
	
$('html').keyup(function(e){
	if(e.keyCode == 46) {
		deleteSelected();
	}
	else if (e.keyCode == 187) {
		add();
	}
});
	
function panZoom() {
	var cx = $flowchart.width() / 2;
	var cy = $flowchart.height() / 2;
		
	$flowchart.panzoom();
	$flowchart.panzoom('pan', -cx + $container.width() / 2, -cy + $container.height() / 2);

	var possibleZooms = [0.5, 0.75, 1, 2, 3];
	var currentZoom = 2;

	$container.on('mousewheel.focal', function( e ) {
		e.preventDefault();
		var delta = (e.delta || e.originalEvent.wheelDelta) || e.originalEvent.detail;
		var zoomOut = delta ? delta < 0 : e.originalEvent.deltaY > 0;
		currentZoom = Math.max(0, Math.min(possibleZooms.length - 1, (currentZoom + (zoomOut * 2 - 1))));
		$flowchart.flowchart('setPositionRatio', possibleZooms[currentZoom]);
		$flowchart.panzoom('zoom', possibleZooms[currentZoom], {
			animate: false,
			focal: e
		});
	});
}
	
function draggable() {
	var $draggableOperators = $('.draggable_operator');
    
	function getOperatorData($element) {
		
		var commands = $element.data('commands');
		
		var nbInputs = parseInt($element.data('inputs'));
		var nbOutputs = parseInt($element.data('outputs'));

		var data = {
		    properties: {
			    title: $element.text(),
			    commands: commands,
			    inputs: {},
			    outputs: {}
		    } 
		};
		  
		var i;
		for (i = 0; i < nbInputs; i++) {
		    data.properties.inputs['input_' + i] = {
			    label: 'I'
		    };
		}
		for (i = 0; i < nbOutputs; i++) {
		    data.properties.outputs['output_' + i] = {
			    label: 'O'
		    };
		}
		  
        return data;
    }
		
	$draggableOperators.draggable({
		cursor: "move",
		opacity: 0.7,
			
		helper: 'clone', 
		appendTo: 'body',
		zIndex: 1000,
			
		helper: function(e) {
			var $this = $(this);
			var data = getOperatorData($this);
			return $flowchart.flowchart('getOperatorElement', data);
		},
		stop: function(e, ui) {
			var $this = $(this);
			var elOffset = ui.offset;
			var containerOffset = $container.offset();
			if (elOffset.left > containerOffset.left &&
				elOffset.top > containerOffset.top && 
				elOffset.left < containerOffset.left + $container.width() &&
				elOffset.top < containerOffset.top + $container.height()) {

				var flowchartOffset = $flowchart.offset();

				var relativeLeft = elOffset.left - flowchartOffset.left;
				var relativeTop = elOffset.top - flowchartOffset.top;

				var positionRatio = $flowchart.flowchart('getPositionRatio');
				relativeLeft /= positionRatio;
				relativeTop /= positionRatio;
					
				var data = getOperatorData($this);
				data.left = relativeLeft;
				data.top = relativeTop;
					
				$flowchart.flowchart('addOperator', data);
			}
		}
	});
}
	
function showEvent(txt) {
    $events.text(txt + "\n" + $events.text());
    $eventsContainer.effect( "highlight", {color: '#3366ff'}, 500);
}

function fromJson(json) {

    if (!json) {
        json = "{}";
    }

    var obj = JSON.parse(json);

    if (obj.operators) {
        operatorI = Object.keys(obj.operators).length;
    }

    $flowchart.flowchart('setData', obj);
}

function toJson() {
	var data = $flowchart.flowchart('getData');
	var json = JSON.stringify(data, null, 2);
	$('#json').val(json);
	$('#json').trigger('autoresize');
	return json;
}

function openLoadFlowModal() {
	$('#save').hide();
	$('#load').show();
	$('#flowModal').openModal();
}

function openSaveFlowModal() {
	$('#save').show();
	$('#load').hide();
	$('#flowModal').openModal();
}

function showError(jqXHR, exception) {
    var error = jqXHR.responseJSON ? jqXHR.responseJSON.details : exception;
    $('#error').text(error);
    $('#errorModal').openModal();
}

function load() {

	var key = $('#flowName').val();
	
	if (!key){
		return;
	}

    $.get('http://' + window.location.hostname + ':' + $('#port').val() + '/api/Persistence?table=flows&key=' + key, function (data) {
        
		if (data) {
			var json = data.Value;
			$('#json').val(json);
			$('#json').trigger('autoresize');
			fromJson(json);
		}
    })
	.fail(function (jqXHR, exception) {
        showError(jqXHR, exception);
    });
}

function save() {
	
	var key = $('#flowName').val();
	
	if (!key){
		return;
	}
	
    var json = toJson();

    $.ajax('/validate',
    {
        data: json,
        contentType: 'application/json',
        type: 'POST'
    })
    .done(function() {
        $.ajax('http://' + window.location.hostname + ':' + $('#port').val() + '/api/Persistence?table=flows&key=' + key,
        {
            data: json,
            contentType: 'application/json',
            type: 'POST'
        })
        .done(function() {
            Materialize.toast('Saved!', 3000, 'blue');
        })
        .fail(function (jqXHR, exception) {
            showError(jqXHR, exception);
        });
    })
	.fail(function (jqXHR, exception) {
	    showError(jqXHR, exception);
    });
}

function validate() {

    var json = toJson();

    $.ajax('/validate',
    {
        data: json,
        contentType: 'application/json',
        type: 'POST'
    })
    .done(function () {
        Materialize.toast('OK!', 3000, 'blue');
    })
	.fail(function (jqXHR, exception) {
	    showError(jqXHR, exception);
	});
}

function clean() {
    var json = null;
    $('#json').val(json);
    $('#json').trigger('autoresize');
    fromJson(json);
}

$(function() {
    
	$flowchart.flowchart({
		data: data,
		onOperatorSelect: function(operatorId) {
		$operatorProperties.show();
		$operatorTitle.val($flowchart.flowchart('getOperatorTitle', operatorId));
		$operatorData.val($flowchart.flowchart('getOperatorData', operatorId).properties.commands);
		
		Materialize.updateTextFields();
		$operatorData.trigger('autoresize');
		$('#modal1').openModal();
		
		showEvent('Operator "' + operatorId + '" selected. Title: ' + $flowchart.flowchart('getOperatorTitle', operatorId) + '.');
		return true;
    },
    onOperatorUnselect: function() {
		$operatorProperties.hide();
		
		$('#modal1').closeModal();
		
		showEvent('Operator unselected.');
		return true;
    },
    onLinkSelect: function(linkId) {
        $linkProperties.show();	
		$linkColor.val($flowchart.flowchart('getLinkMainColor', linkId));
		
		$('#modal1').openModal();
		
		showEvent('Link "' + linkId + '" selected. Main color: ' + $flowchart.flowchart('getLinkMainColor', linkId) + '.');
		return true;
    },
    onLinkUnselect: function() {
		$linkProperties.hide();
		
		$('#modal1').closeModal();
		
		showEvent('Link unselected.');
		return true;
    },
    onOperatorCreate: function(operatorId, operatorData, fullElement) {
		showEvent('New operator created. Operator ID: "' + operatorId + '", operator title: "' + operatorData.properties.title + '".');
		return true;
    },
    onLinkCreate: function(linkId, linkData) {
		showEvent('New link created. Link ID: "' + linkId + '", link color: "' + linkData.color + '".');
		return true;
    },
    onOperatorDelete: function(operatorId) {
		showEvent('Operator deleted. Operator ID: "' + operatorId + '", operator title: "' + $flowchart.flowchart('getOperatorTitle', operatorId) + '".');
		return true;
    },
    onLinkDelete: function(linkId, forced) {
		showEvent('Link deleted. Link ID: "' + linkId + '", link color: "' + $flowchart.flowchart('getLinkMainColor', linkId) + '".');
		return true;
    },
    onOperatorMoved: function(operatorId, position) {
		showEvent('Operator moved. Operator ID: "' + operatorId + ', position: ' + JSON.stringify(position) + '.');
		}
	});
		
	$operatorTitle.keyup(function() {
		var selectedOperatorId = $flowchart.flowchart('getSelectedOperatorId');
		if (selectedOperatorId != null) {
			$flowchart.flowchart('setOperatorTitle', selectedOperatorId, $operatorTitle.val());
		}
	});
		
	$operatorData.keyup(function() {
		var selectedOperatorId = $flowchart.flowchart('getSelectedOperatorId');
		if (selectedOperatorId != null) {	  
			$flowchart.flowchart('setOperatorCommands', selectedOperatorId, $operatorData.val());
			
			$flowchart.flowchart('redrawLinksLayer');	
		}
	});
		
	$linkColor.change(function() {
		var selectedLinkId = $flowchart.flowchart('getSelectedLinkId');
		if (selectedLinkId != null) {
			$flowchart.flowchart('setLinkMainColor', selectedLinkId, $linkColor.val());
		}
	});
		
	//panZoom();
	draggable();
 
	$('#flowName').val('test');
	$('#flowName').keydown(function (e) {
	    if (e.keyCode == 13) {
	        if ($('#save').is(':visible')) {
	            save();
	        } else {
	            load();
	        }

	        $('#flowModal').closeModal();
	    }
	});

	load();	
});
