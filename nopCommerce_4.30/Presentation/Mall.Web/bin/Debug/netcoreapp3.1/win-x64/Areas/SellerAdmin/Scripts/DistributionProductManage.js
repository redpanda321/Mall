var loading;
var categoryId = 0;
var maxBrokerageRate = 0;
var maxLevel = 0;
$(function () {
    maxBrokerageRate = $("#maxBrokerageRate").val();
    maxLevel = $("#maxLevel").val();

    LoadData();

    bindSearchBtnClick();

    $('#category1,#category2').MallLinkage({
        url: '/SellerAdmin/Category/GetCategory',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function (level, value, text) {
            categoryId = value;
        }
    });

    $("#addBtn").click(function () {
        loading = showLoading();
        $.post('./GetAllProductIds', null, function (data) {
            loading.close();
            $.productSelector.params.isShopCategory = true;
            $.productSelector.show(data, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.id);
                });
                loading = showLoading();
                $.post('./AddProducts', { ids: ids.toString(), platformType: 1 }, function (data) {
                    loading.close();
                    if (data.success) {
                        //$("#list").MallDatagrid('reload', {});
                        reload(1);
                    }
                    else {
                        $.dialog.alert('添加分销推广商品失败!' + data.msg);
                    }
                });
            }, 'selleradmin', null, null, null, 'distribution');
        });
    });

    CancelEventBind();
    RateTextEventBind();
});

function CancelEventBind() {
    $('#list').on('click', '.btnCancel', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要取消这件商品的分销推广吗？', function () {
            loading = showLoading();
            $.post('./CancelProduct', { id: ids }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('取消推广成功');
                    var pageNo = $("#list").MallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else {
                    $.dialog.alert('取消推广失败!' + result.msg);
                }
            });
        });
    });

}

//佣金修改
function RateTextEventBind() {
    $('#list').on('blur', '.disrate', function () {
        var _t = $(this);
        var _p = _t.parents(".disratebox");
        var _alldr = $(".disrate", _p);
        var maxBrokerageRate = parseFloat($("#maxBrokerageRate").val());
        var rate1 = null, rate2 = null, rate3 = null;
        var productId = _t.data('proid');
        var isdataok = true;
        var needupdate = false;
        _alldr.each(function (ind, itemd) {
            var item = $(itemd);
            item.removeClass("checkError");
            var level = item.data('level');
            var _rate = item.attr('oriValue');
            _rate = parseFloat(_rate);
            var value = $.trim(item.val());
            if (!value) {
                isdataok = false;
                item.addClass("checkError");
                return;
            }
            else {
                var currate = parseFloat(value);
                if (isNaN(value) || currate <= 0 || currate > maxBrokerageRate) {
                    isdataok = false;
                    item.addClass("checkError");
                    return;
                }
                if (/^\d{1,2}(\.\d)?$/g.test(value)) {
                    if (currate != _rate) {
                        needupdate = true;
                    }
                    switch (level) {
                        case 1:
                            rate1 = value;
                            break;
                        case 2:
                            rate2 = value;
                            break;
                        case 3:
                            rate3 = value;
                            break;
                    }
                } else {
                    isdataok = false;
                    item.addClass("checkError");
                    return;
                }
            }

        });
        if (!isdataok) {
            $.dialog.errorTips("总比例需在 0.1% ~ " + maxBrokerageRate + "% 之间.仅可保留一位小数");
        }
        if (needupdate && isdataok) {
            updateRate(productId, rate1, rate2, rate3);
        }

    });
}

function updateRate(id, rate1, rate2, rate3) {
    var loading = showLoading();
    $.post('./updateRate', { id: id, rate1: rate1, rate2: rate2, rate3: rate3 }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips('更新佣金比例成功');
            var pageNo = $("#list").MallDatagrid('options').pageNumber;
            reload(pageNo);
        }
        else {
            $.dialog.alert('调整佣金比例失败!' + result.msg);
        }
    });
}

function bindSearchBtnClick() {
    $('#searchBtn').click(function () {
        var pageNo = $("#list").MallDatagrid('options').pageNumber;
        reload(pageNo);
    });
}

function reload(pageNo) {
    var seakey = $.trim($('#skey').val());
    $("#list").MallDatagrid('reload', { skey: seakey, categoryId: categoryId, pageNumber: pageNo });
}

function LoadData() {
    $("#list").html('');

    //商品表格
    $("#list").MallDatagrid({
        url: '/SellerAdmin/DistributionProduct/GetProductList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的商品',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        operationButtons: "#batbtnbox",
        columns:
        [[
            {
                checkbox: true, width: 40,
            },
            {
                field: "ProductName", title: '商品', width: 150, align: 'center',
                formatter: function (value, row, index) {
                    var html = '<span class="overflow-ellipsis" style="width:300px"><a title="' + value + '" target="_blank" href="/product/detail/' + row.ProductId + '">' + value + '</a></span>';
                    return html;
                }
            },
            {
                field: "BrokerageRate1", title: "佣金比例", width: 120, align: "center",
                formatter: function (value, row, index) {
                    var html = '<div class="disratebox lh30">';
                    html += '一级 <input class="text-order no-m disrate w40 lh20" type="text" data-proid="' + row.ProductId + '" data-level="1" value="' + value + '" oriValue="' + value + '"> %<br/>';
                    if (maxLevel > 1) {
                        html += '二级 <input class="text-order no-m disrate w40 lh20" type="text" data-proid="' + row.ProductId + '" data-level="2" value="' + row.BrokerageRate2 + '" oriValue="' + row.BrokerageRate2 + '"> %<br/>';
                    }
                    if (maxLevel > 2) {
                        html += '三级 <input class="text-order no-m disrate w40 lh20" type="text" data-proid="' + row.ProductId + '" data-level="3" value="' + row.BrokerageRate3 + '" oriValue="' + row.BrokerageRate3 + '"> %<br/>';
                    }
                    html += '</div>';
                    return html;
                }
            },
            {
                field: "CategoryName", title: "商家分类", align: "center"
            },
            {
                field: "MinSalePrice", title: "价格", align: "center"
            },
            {
                field: "s", title: "操作", width: 150, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.ProductId + '"/><input class="thisName" type="hidden" value="' + row.ProductName + '"/>';
                    html += '<a class="btnCancel">取消推广</a></span>';
                    return html;
                }
            }
        ]]
    });
}

function getSelectedIds() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.ProductId);
    });
    return ids;
}
function getSelectedNames() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var productNames = [];
    $.each(selecteds, function () {
        productNames.push(this.ProductName);
    });
    return productNames;
}
///批量取消推广
function BatchCancel() {
    var ids = getSelectedIds();
    if (ids.length <= 0) {
        $.dialog.tips('请至少选择一个分销商品');
        return false;
    }

    $.dialog.confirm('您确定要取消这些商品的分销推广吗？', function () {
        loading = showLoading();
        $.post('./CancelProductList', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('批量取消推广成功');
                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else {
                $.dialog.alert('批量取消推广失败!' + result.msg);
            }
        });
    });
}