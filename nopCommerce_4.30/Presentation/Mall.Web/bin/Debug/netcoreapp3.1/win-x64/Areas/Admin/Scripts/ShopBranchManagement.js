var shopId;
$(function () {
    $("#area-selector").RegionSelector({
        selectClass: "form-control input-sm select-sort",
        valueHidden: "#AddressId"
    });
    $('#searchButton').click(function () {
        var para = GetQueryPara();
        $("#shopDatagrid").MallDatagrid('clearReload', para);
    });
    shopId = GetQueryString("shopId");
    if (shopId)
        $("#ShopId").val(shopId);
    LoadData();
});
function GetQueryPara() {
    var para = {};
    para.shopBranchName = $.trim($('#shopBranchName').val());
    para.contactPhone = $.trim($('#contactPhone').val());
    para.contactUser = $.trim($('#contactUser').val());
    para.ShopBranchTagId = $.trim($('#ShopBranchTagId').val());
    para.ShopId = $.trim($('#ShopId').val());

    if ($.trim($('#AddressId').val()) != '0')
        para.AddressId = $.trim($('#AddressId').val());
    return para;
}
function reloadDataGrid() {
    var para = GetQueryPara();
    $("#shopDatagrid").MallDatagrid('clearReload', para);
}
function LoadData() {
    var para = {};
    para.shopBranchName = '';
    para.contactPhone = '';
    para.contackUser = '';
    para.ShopBranchTagId = $.trim($('#ShopBranchTagId').val());
    para.ShopId = shopId;
    $("#shopDatagrid").MallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的门店',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: para,
        operationButtons: "#saleOff",
        operationButtons: "#batchSetTags",
        columns:
        [[
            { checkbox: true, width: 39 },
            {
                field: "ShopBranchName", title: '门店名称', width: 150, align: 'center'
            },
            {
                field: "ShopName", title: "商家", width: 150, align: "center"
            },
            {
                field: "ShopBranchInTagNames", title: "标签", width: 150, align: "center"
            },
            {
                field: "ContactUser", title: "联系人", width: 150, align: "center"
            },
            {
                field: "ContactPhone", title: "联系方式", width: 150, align: "center"
            },
            {
                field: "AddressFullName", title: '门店地址', width: 400, align: 'center'
            },
            {
                field: "s", title: "操作", width: 150, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.Name + '"/>';
                    if (row.Status == 0) {
                        html += '<a class="good-down" onclick="Freeze(' + row.Id + ')">冻结</a>';
                    } else {
                        html += '<a class="good-down" onclick="UnFreeze(' + row.Id + ')">解冻</a>';
                    }                    
                    html += '<a class="good-check"  href="/admin/order/management?shopBranchId=' + row.Id + '">订单</a>';
                    html += '<a class="good-check"  href="ProductManagement?shopBranchId=' + row.Id + '">商品</a>';
                    html += '<a class="good-del" onclick="StoresLink(' + row.Id + ')">链接</a>';
                    return html;
                }
            }
        ]],
        onLoadSuccess: function () {
        }
    });
}

function Freeze(shopBranchId) {
    $.dialog.confirm('冻结之后，门店管理员将不能登录门店后台且门店也不能做为自提点，您确定要冻结？', function () {
        var loading = showLoading();
        $.ajax({
            type: 'post',
            url: 'Freeze',
            async: false,
            data: { shopBranchId: shopBranchId },
            success: function (result) {
                if (loading)
                    loading.close();
                $.dialog.alert(result.msg);
                reloadDataGrid();
            }
        });
    });
}
function UnFreeze(shopBranchId) {
    $.dialog.confirm('门店现在为冻结状态，确定解冻门店吗?', function () {
        var loading = showLoading();
        $.ajax({
            type: 'post',
            url: 'UnFreeze',
            async: false,
            data: { shopBranchId: shopBranchId },
            success: function (result) {
                if (loading)
                    loading.close();
                $.dialog.alert(result.msg);
                reloadDataGrid();
            }
        });
    });
}
//门店链接
function StoresLink(shopBranchId) {
    var url = window.location.protocol + "//" + window.location.host + '/m-wap/shopbranch/index/' + shopBranchId;
    $("#referralsLink").val(url);
    var loading = showLoading();
    $.dialog({
        title: '门店链接',
        lock: true,
        id: 'StoresLink',
        content: document.getElementById("storesLink-form"),
        padding: '0 40px',
        init: function () {
            $.ajax({
                type: 'GET',
                url: 'StoresLink',
                cache: false,
                data: { 'vshopUrl': url },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $("#imgsrc").attr("src", data.qrCodeImagePath);
                    }
                }, error: function () {
                    loading.close();
                }
            });
        }
    });
}

function batchSetTags() {
    var selectedRows = $("#shopDatagrid").MallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].Id);
    }
    if (selectedRows.length == 0) {
        $.dialog.errorTips("你没有选择任何选项！");
    }
    else {

        $.dialog({
            title: '批量设置标签',
            id: 'addManager',
            content: document.getElementById("setTagsform"),
            lock: true,
            okVal: '保存',
            padding: '0 40px',
            init: function () {
                $("#txtShopId").val(selectids.join(','));
            },
            ok: function () {
                var shopIds = $("#txtShopId").val();
                var selectTagids = new Array();
                $('input:checkbox[name=chkTags]:checked').each(function(i){
                    selectTagids.push($(this).val());
                });
                if (selectTagids.length == 0) {
                    $.dialog.errorTips("你没有选择任何选项！");
                }
                SetTag(shopIds, selectTagids.join(','));
            }
        });
    }
}
function SetTag(shopIds, tagIds) {
    var loading = showLoading();
    $.ajax({
        type: 'post',
        url: 'SetShopBranchTags',
        cache: false,
        async: true,
        data: { shopIds: shopIds, tagIds: tagIds },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                $.dialog.tips("设置成功！");
                $('input:checkbox[name=chkTags]:checked').attr("checked", false);

                var para = GetQueryPara();
                $("#shopDatagrid").MallDatagrid('clearReload', para);
            }
            else {
                $.dialog.errorTips("设置失败！" + data.msg);
            }
        },
        error: function () {
            loading.close();
        }
    });
}