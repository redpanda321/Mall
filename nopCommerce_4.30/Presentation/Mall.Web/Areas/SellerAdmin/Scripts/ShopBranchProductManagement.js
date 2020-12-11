$(function () {
    $('#searchButton').click(function (e) {
        reloadDataGrid();
    });
    LoadData();
});
function GetQueryPara()
{
    var para = {};
    para.ShopBranchId = GetQueryString("shopBranchId");
    para.KeyWords = $.trim($('#shopBranchName').val());
    var cid = $('#shopCategory').val();
    if (cid > 0) para.ShopCategoryId = cid;
    return para;
}
function reloadDataGrid()
{
    var para = GetQueryPara();
    $("#shopDatagrid").MallDatagrid('clearReload', para);
}
function LoadData()
{
    var para = {};
    para.ShopBranchId = GetQueryString("shopBranchId");
    $("#shopDatagrid").MallDatagrid({
        url: 'ProductList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的商品',
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
        columns:
        [[
            { checkbox: true, width: 40 },
            {
                field: "Name", title: '商品', align: 'left',width: 200,
                formatter: function (value, row, index) {
                	var html = '';
                	if (row.ProductType == 1) {
                        html = '<img class="pull-right" src="/Images/virtual.png"/>';
                    }
                	html += '<img class="ml15 mr10 pull-left" width="40" height="40" src="' + row.Image + '" /><a class="single-ellipsis w100 h40 lh40" title="' + value + '" href="/m-wap/branchproduct/detail/' + row.Id + '?shopBranchId=' + para.ShopBranchId + '" target="_blank">' + value + '</a>';
                    return html;
                }
            },
            { field: "CategoryName", title: "商家分类", width: 90, align: "center" },
        {
            field: "Price", title: "门店价格", width: 150, align: "center",
            formatter: function (value, row, index) {
                var html = row.Price.toFixed(2);
                if(row.MinPrice != row.MaxPrice){
                    html = row.MinPrice.toFixed(2) + '-' + row.MaxPrice.toFixed(2);
                }
                return html;
            }
        },
        {
            field: "SaleCount", title: "销量", width: 150, align: "center"
        },
        { field: "Stock", title: "门店库存", width: 90, align: "center" },
        {
            field: "s", title: "操作", width: 150, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.Name + '"/>';
                html += '<a class="good-check"  onclick="batchSettingStock(' + row.Id + ')">编辑</a>';
                html += '<a class="good-del" onclick="StoresLink(' + row.Id + ')">链接</a>';
                html += '<a class="good-check"  onclick="doBatchUnSale(' + row.Id + ',1)">下架</a>';
                return html;
            }
        }
        ]],
        onLoadSuccess: function () {
        }
    });
}

//下架
function doBatchUnSale(ids, num) {
    var ShopBranchId = GetQueryString("shopBranchId");
    $.dialog.confirm('您确定要下架这' + (num ? num : '') + '个商品吗?', function () {
        var loading = showLoading();
        $.post('GetUnSaleProduct', { pids: ids.toString(), shopbranchId: ShopBranchId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('下架商品成功');
                reloadDataGrid();
            }
            else
                $.dialog.alert('下架商品失败!' + result.msg);
        });
    });
}
//批量下架
function SaleOff() {
    var ids = getSelectedIds();
    if (ids.length > 0)
    {
        doBatchUnSale(ids, ids.length);
    } else
        $.dialog.tips('请至少选择一件商品');
}

//商品链接
function StoresLink(shopBranchId) {
    var url = window.location.protocol + "//" + window.location.host + '/m-wap/product/detail/' + shopBranchId;
    $("#referralsLink").val(url);
    var loading = showLoading();
        $.dialog({
            title: '商品链接',
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

//批量编辑
function bachEdit() {
    var ids = getSelectedIds();
    if (ids.length > 0) {
        batchSettingStock(ids);
    } else
        $.dialog.tips('请至少选择一件商品');
}
//编辑库存
function batchSettingStock(ids) {
    $("input[type='text'][id='updateStock']").val('');
    $("#btnSaveStock").unbind('click').bind("click", function () {
        var updateStock = Number($.trim($('#updateStock').val()));
        $(".j_inputStock").each(function () {
            if (Number($(this).val()) > 0 || updateStock > 0) {
                if ((Number($(this).val()) + updateStock) <= 0) {
                    $(this).val(0);
                } else {
                    $(this).val(updateStock);
                }
            }
        });
    });
    loadProducts(ids);
    $.dialog({
        title: '商品编辑',
        lock: true,
        id: 'batchSettingStock',
        content: document.getElementById("batchSettingStock"),
        //padding: '0 20px',
        okVal: '保存',
        ok: function () {
            var close = false;
            var loading = showLoading();
            var stocks = [];
            var flag = true;
            $(".j_inputStock").each(function () {
                if ($.trim($(this).val()) == "") {
                    $.dialog.errorTips('请输入大于等于0的整数!');
                    flag = false;
                    loading.close();
                    $(this).parent().addClass('has-error');
                    return;
                } else {
                    flag = true;
                    $(this).parent().removeClass('has-error');
                }
                var info = new Object();
                info.SkuId = $(this).attr("skuid");
                info.Stock = Number($(this).val());
                stocks.push(info);
            });
            if (flag) {
                $.ajax({
                    type: "post",
                    url: "batchSettingStock",
                    data: { stocks: stocks, shopbranchId: GetQueryString("shopBranchId") },
                    dataType: "json",
                    async: false,
                    success: function (result) {
                        loading.close();
                        if (result.success) {
                            $.dialog.succeedTips('设置成功');
                            reloadDataGrid();
                            close = true;
                        }
                        else
                            $.dialog.errorTips('设置异常!' + result.msg);
                    },
                    error: function () {
                        loading.close();
                        $.dialog.errorTips('设置出错！');
                    }
                });
            }
            return close;
        }
    });
}

//编辑加载商品
function loadProducts(ids) {
    $.ajax({
        type: "GET",
        url: "GetProductsByIds",
        data: { ids: ids.toString(), shopbranchId: GetQueryString("shopBranchId") },
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.success == false) {
                $.dialog.errorTips("获取商品数据错误！");
            } else {
                var databox = $(".j_productResult");
                databox.empty();
                if (data.model != null) {
                    var html = '';
                    if (data.model && data.model.length > 0) {
                        $.each(data.model, function (i, model) {
                            $.each(model.Skus, function (j, sku) {
                                html = '<tr>';
                                if (j == 0) {
                                    html += '  <td rowspan="' + model.Skus.length + '">';
                                    html += '     <img src="' + model.Image + '" />';
                                    html += '     <a>' + model.Name + '</a>';
                                    html += '  </td>';
                                }
                                html += '  <td>' + sku.ProductName + '</td>';
                                html += '  <td>' + sku.SalePrice.toFixed(2) + '</td>';
                                html += '  <td>' + sku.SalePrice.toFixed(2) + '</td>';
                                html += '  <td>';
                                html += '     <input class="form-control input-sm w95 j_inputStock" placeholder="库存" for="Stock" value=' + sku.Stock + ' skuid="' + sku.Id + '" />';
                                html += ' </td>';
                                html += '</tr>';
                                databox.append(html);
                            });     
                        });
                    }
                }
                $(".j_inputStock").bind("keyup", function () {
                    $(this).val($(this).val().replace(/[^0-9]/g, ''));
                });
                $(".j_inputStock").bind("blur", function () {
                    if ($.trim($(this).val()) == "") {
                        $.dialog.errorTips('请输入大于等于0的整数!');
                        $(this).parent().addClass('has-error');
                    } else {
                        $(this).parent().removeClass('has-error');
                    }
                });
            }
        },
        error: function () {
            $.dialog.errorTips("系统繁忙，请刷新重试");
        }
    });
}

function getSelectedIds() {
    var selecteds = $("#shopDatagrid").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.Id);
    });
    return ids;
}

//上架商品
function SaleOnProduct() {
    $.post('ShopBranchProductIds', { shopbranchId: GetQueryString("shopBranchId") }, function (data) {
        $.productSelector.params.isShopCategory = true;
        $.productSelector.show(null, function (selectedProducts) {
            var ids = [];
            $.each(selectedProducts, function () {
                ids.push(this.id);
            });
            var loading = showLoading();
            $.post('OnSaleProduct', { shopbranchId: GetQueryString("shopBranchId"), pids: ids.toString() }, function (data) {
                loading.close();
                if (data.success)
                    reloadDataGrid();
                else
                    $.dialog.errorTips(data.msg);
            });
        }, 'shopbranch', null, data);
    });
}