
$(function () {

    initExpressIcon();
    bindSendBtnClickEvent();
    // initAddressSelector();
    $("#regionSelector").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#SenderRegionId"
    });
})

function initAddressSelector() {
    var fullRegionPath = $('#fullRegionPath').val();
    fullRegionPath = fullRegionPath.split(',');
    $('#senderAddressProvince,#senderAddressCity,#senderAddressDistrict').MallLinkage({
        url: '/common/RegionAPI/GetRegion',
        enableDefaultItem: true,
        defaultItemsText: '请选择',
        defaultSelectedValues: fullRegionPath,
        onChange: function (level, value, text) {
            if (level == 2)
                $('#regionId').val(value);
        }
    });
}


function initExpressIcon() {
    $('.express-choose li').live('click', function () {
        if ($(this).attr('notSet'))
            $.dialog.tips($(this).attr('title') + ' 的快递模板还未被设置，请联系平台进行设置');
        else
            $(this).addClass('active').siblings().removeClass('active');
    });

    $('.more-express').click(function () {
        $(this).hide().parent().removeClass().addClass('col-sm-9');
        $(this).siblings().addClass('height-auto');
    });

}

function bindSendBtnClickEvent() {
    $('#print-express').click(function () {
        try {
            printOrder();
        }
        catch (e) {
            $.dialog.tips(e.message);
        }
    });
}


function printOrder() {

    var orderIds = QueryString('orderIds');

    var expressName = $('li[expressCompany].active').attr('name');
    if (!expressName)
        throw new Error('请选择一个快递公司');

    var startNo = $('#startNo').val();
    if (!startNo)
        throw new Error('请输入起始快递单号');

    var loading = showLoading();
    $.post('print',
        { orderIds: orderIds, expressName: expressName, startNo: startNo },
        function (data) {
            loading.close();
            if (data.success) {
                $.each(data.data, function (i, printData) {
                    LodopPrint(printData.Width, printData.Height, printData.FontSize, printData.Elements, false, false, true);//width,height,fontSize,data,preview
                });
            }
            else {
                $.dialog.errorTips(data.msg);
            }

        }
    );

}
