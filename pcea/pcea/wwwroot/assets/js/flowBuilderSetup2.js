﻿$(document).ready(function () {
    var data = {
        operators: {
            operator1: {
                top: 20,
                left: 20,
                properties: {
                    title: 'Operator 1',
                    inputs: {},
                    outputs: {
                        output_1: {
                            label: 'Output 1',
                        }
                    }
                }
            },
            operator2: {
                top: 80,
                left: 300,
                properties: {
                    title: 'Operator 2',
                    inputs: {
                        input_1: {
                            label: 'Input 1',
                        },
                        input_2: {
                            label: 'Input 2',
                        },
                    },
                    outputs: {}
                }
            },
        },
        links: {
            link_1: {
                fromOperator: 'operator1',
                fromConnector: 'output_1',
                toOperator: 'operator2',
                toConnector: 'input_2',
            },
        }
    };

    var $operatorProperties = $('#operator_properties');
    var $linkProperties = $('#link_properties');
    var $operatorTitle = $('#operator_title');
    var $linkColor = $('#link_color');

    // Apply the plugin on a standard, empty div...
    var $flowchart = $('#flowchartworkspace');
    $flowchart.flowchart({
        data: data,
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

    var operatorI = 0;
    $flowchart.siblings('.create_operator').click(function () {
        var operatorId = 'created_operator_' + operatorI;
        var operatorData = {
            top: 60,
            left: 500,
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

    $flowchart.siblings('.delete_selected_button').click(function () {
        $flowchart.flowchart('deleteSelected');
    });

    $flowchart.siblings('.get_data').click(function () {
        var data = $flowchart.flowchart('getData');
        $('#flowchart_data').val(JSON.stringify(data, null, 2));
    });

    $flowchart.siblings('.set_data').click(function () {
        var data = JSON.parse($('#flowchart_data').val());
        $flowchart.flowchart('setData', data);
    });
});