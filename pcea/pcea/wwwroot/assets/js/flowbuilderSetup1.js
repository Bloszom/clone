/* global $ */
$(document).ready(function () {
    var $flowchart = $('#flowchartworkspace');
    var $container = $flowchart.parent();



    // Apply the plugin on a standard, empty div...
    $flowchart.flowchart({
        verticalConnection: true,
        data: defaultFlowchartData,
        defaultSelectedLinkColor: '#000055',
        grid: 10,
        multipleLinksOnInput: false,
        multipleLinksOnOutput: false
    });


    function getOperatorData($element) {
        var nbInputs = parseInt($element.data('nb-inputs'), 10);
        var nbOutputs = parseInt($element.data('nb-outputs'), 10);
        var data = {
            properties: {
                title: $element.text(),
                inputs: {},
                outputs: {}
            }
        };

        var i = 0;
        for (i = 0; i < nbInputs; i++) {
            data.properties.inputs['input_' + i] = {
                label: 'Input ' + (i + 1)
            };
        }
        for (i = 0; i < nbOutputs; i++) {
            data.properties.outputs['output_' + i] = {
                label: 'Output ' + (i + 1)
            };
        }

        return data;
    }

    //-----------------------------------------
    //--- operator and link properties
    //--- start
    var $operatorProperties = $('#operator_properties');
    $operatorProperties.hide();
    var $linkProperties = $('#link_properties');
    $linkProperties.hide();
    var $operatorTitle = $('#operator_title');
    var $linkColor = $('#link_color');

    $flowchart.flowchart({
        onOperatorSelect: function (operatorId) {
            $operatorProperties.show();
            $operatorTitle.val($flowchart.flowchart('getOperatorTitle', operatorId));
            return true;
        },
        onOperatorUnselect: function () {
            $operatorProperties.hide();
            return true;
        },
        onLinkSelect: function (linkId) {
            $linkProperties.show();
            $linkColor.val($flowchart.flowchart('getLinkMainColor', linkId));
            return true;
        },
        onLinkUnselect: function () {
            $linkProperties.hide();
            return true;
        }
    });

    $operatorTitle.keyup(function () {
        var selectedOperatorId = $flowchart.flowchart('getSelectedOperatorId');
        if (selectedOperatorId != null) {
            $flowchart.flowchart('setOperatorTitle', selectedOperatorId, $operatorTitle.val());
        }
    });

    $linkColor.change(function () {
        var selectedLinkId = $flowchart.flowchart('getSelectedLinkId');
        if (selectedLinkId != null) {
            $flowchart.flowchart('setLinkMainColor', selectedLinkId, $linkColor.val());
        }
    });
    //--- end
    //--- operator and link properties
    //-----------------------------------------

    //-----------------------------------------
    //--- delete operator / link button
    //--- start
    //$('html').keyup(function (e) {
    //    if (e.keycode == 46)
    //        $flowchart.flowchart('deleteSelected');
    //});

    //$flowchart.siblings('.delete_selected_button').click(function () {
    //    alert(clicked);
    //    $flowchart.flowchart('deleteSelected');
    //});
    //$flowchart.parent.siblings('.row .delete_selected_button').click(function () {
    //    alert(clicked);
    //    $flowchart.flowchart('deleteSelected');
    //});

    $('button.delete_selected_button').on('click', function () {
        console.log('clicked');
        $flowchart.flowchart('deleteSelected');

    });
    //document.getElementById("button.delete_selected_button").click(function () {
    //    alert('Button clicked');

    //})

    //$flowchart.parent().siblings('.delete_selected_button').click(function () {
    //    alert('Button clicked');
    //    $flowchart.flowchart('deleteSelected');
    //});
    //--- end
    //--- delete operator / link button
    //-----------------------------------------



    //-----------------------------------------
    //--- create operator button
    //--- start
    var operatorI = 0;
    $flowchart.parent().siblings('.create_operator').click(function () {
        var operatorId = 'created_operator_' + operatorI;
        var operatorData = {
            top: ($flowchart.height() / 2) - 30,
            left: ($flowchart.width() / 2) - 100 + (operatorI * 10),
            properties: {
                title: 'Operator ' + (operatorI + 3),
                inputs: {
                    input_1: {
                        label: 'Input 1',
                    }
                },
                outputs: {
                    output_1: {
                        label: 'Output 1',
                    }
                }
            }
        };

        operatorI++;

        $flowchart.flowchart('createOperator', operatorId, operatorData);

    });
    //--- end
    //--- create operator button
    //-----------------------------------------




    //-----------------------------------------
    //--- draggable operators
    //--- start
    //var operatorId = 0;
    var $draggableOperators = $('.draggable_operator');
    $draggableOperators.draggable({
        cursor: "move",
        opacity: 0.7,

        //helper: 'clone',
        appendTo: 'body',
        zIndex: 1000,

        helper: function (e) {
            var $this = $(this);
            var data = getOperatorData($this);
            return $flowchart.flowchart('getOperatorElement', data);
        },
        stop: function (e, ui) {
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
    //--- end
    //--- draggable operators
    //-----------------------------------------


    //-----------------------------------------
    //--- save and load
    //--- start
    $('.get_data').click(function () {
        // Task is to execute callbackFirst 
        // function first and then execute 
        // callbackSecond function. 
        callbackFirst();
    });

    function callbackFirst() {
        var data = $flowchart.flowchart('getData')
        $("#flowchart_data").val(JSON.stringify(data, null, 2));

        // Execute callbackSecond() now as its 
        // the end of callbackFirst() 
        callbackSecond();
    }

    function callbackSecond() {
        var id = document.getElementById('processid');
        var workflow = new Object();
        workflow.processdata = $("#flowchart_data").val();
        workflow.processid = $(id).text();
        var encode = encodeURIComponent(workflow.processdata);

        var operator = [];
        var links = [];

        var data = JSON.parse(workflow.processdata);
        for (var element in data.operators) {
            var value = data.operators[element].properties['title'];
            operator.push({ processid: $(id).text(), actorName: value.trim(), actorNumber: parseInt(element) });
        };

        for (var element in data.links) {
            var fromOperator = data.links[element]['fromOperator'];
            var toOperator = data.links[element]['toOperator'];
            links.push({ processid: $(id).text(), linkId: parseInt(element), fromActorNumber: parseInt(fromOperator), toActorNumber: parseInt(toOperator) });
        }

        if (workflow != null) {

            //xhttp.open("POST", "/Workflows/SaveFlowData", true);
            //xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            //xhttp.send("{ sProcessdata: encode, sProcessid: workflow.processid, workflowActors: operator, workflowLinks: links }");


            $.ajax({
                type: "post",
                url: "/Workflows/SaveFlowData",
                data: { sProcessdata: encode, sProcessid: workflow.processid, "workflowActors": operator, "workflowLinks": links },
                dataType: "json",
                success: function (response) {
                    alert("Record Saved Successfully");
                },
                error: function (response) {
                    if (response.status == 200) {
                        alert("Workflow Saved");
                        window.location.reload();
                    }
                    else
                        alert(response.statusText + " " + response.status);
                }
            });
            //Executes callbackSecond()
            //ends the callback functions
        }
    }


    function Text2Flow() {
        var data = JSON.parse($('#flowchart_data').val());
        $flowchart.flowchart('setData', data);
    };

    //Text2Flow;
    Text2Flow();



    /*global localStorage*/
    function SaveToLocalStorage() {
        if (typeof localStorage !== 'object') {
            alert('local storage not available');
            return;
        }
        Flow2Text();
        localStorage.setItem("stgLocalFlowChart", $('#flowchart_data').val());
    }
    $('#save_local').click(SaveToLocalStorage);

    function LoadFromLocalStorage() {
        if (typeof localStorage !== 'object') {
            alert('local storage not available');
            return;
        }
        var s = localStorage.getItem("stgLocalFlowChart");
        if (s != null) {
            $('#flowchart_data').val(s);
            Text2Flow();
        }
        else {
            alert('local storage empty');
        }
    }
    $('#load_local').click(LoadFromLocalStorage);
    //--- end
    //--- save and load
    //-----------------------------------------


});

var defaultFlowchartData = {

};
if (false) console.log('remove lint unused warning', defaultFlowchartData);