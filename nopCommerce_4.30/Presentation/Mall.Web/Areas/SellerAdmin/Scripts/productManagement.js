/// <reference path="../../../Scripts/jqeury.MallLinkage.js" />


var AuditOnOff = 0;
var _saleStatus = '1';
var _auditStatus = [2];
//$(function () {
function showQrCode(imgSrc) {
    $.dialog({
        title: '二维码',
        lock: true,
        id: 'weixinQrCode',
        content: $("#weixinQrCode").html(),
        padding: '0 40px',
        init: function () {
            $("#QrCode").attr('src', imgSrc)
        }
    });
}
AuditOnOff = $("#VBAO").val();
//});

var categoryId;
var lastType;
var curType;
var fgstock;
var freightTemplateId;
$(function () {
    fgstock = $('.fg-stock input:checkbox').get(0);

    bindTabSwich();
    if (val == '') {
        initGrid();
    }
    initDatePicker();
    bindSearchBtnClick();
    initCategoryLinkage();
    initBrandAutoComplete();
    bindAssociateTemplateBtnClickEvent();

    $('#list').on('click', '.good-down', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要下架' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件商品')) + '吗？', function () {
            var loading = showLoading();
            $.post('batchSaleOff', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('下架商品成功');
                    var pageNo = $("#list").MallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('下架商品失败!' + result.msg);
            });
        });


    });

    $('#list').on('click', '.good-up', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        if (curType == 'saleOff' && AuditOnOff == 0) {
            $.dialog.alert('违规下架的商品，不能再上架!');
            return;
        }
        $.dialog.confirm('您确定要上架' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件商品')) + '吗？', function () {
            var loading = showLoading();
            $.post('batchOnSale', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('申请商品上架成功');
                    var pageNo = $("#list").MallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('申请商品上架失败!' + result.msg);
            });
        });
    });

    $('#list').on('click', '.good-del', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要删除' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件商品')) + '吗？', function () {
            var loading = showLoading();
            $.post('Delete', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('删除商品成功');
                    var pageNo = $("#list").MallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('删除商品失败!' + result.msg);
            });
        });
    });

    $('#list').on('hover', '.good-share', function () {
        $(this).toggleClass('active');
    });

    $(document).on('click', '.recommend', function () {
        var $this = $(this);
        var relationProducts = $this.data('relation-products').toString();
        var exceptProducts = [];
        exceptProducts.push($this.data('product-id'));
        var productids = relationProducts.split(',');
        $.productSelector.show(productids, function (selectedProducts) {
            if (selectedProducts.length > 10) {
                $.dialog.errorTips('商品最大数不能超过10！');
                return;
            }
            relationProducts = selectedProducts.newitem(function (p) { return p.id; }).join(',');
            $this.data('relation-products', relationProducts);

            $.get('/selleradmin/product/Recommend?productId={0}&productIds={1}'.format($this.data('product-id'), relationProducts), function (data) {
                if (data && data.success == true)
                    $.dialog.succeedTips('操作成功！');
                else
                    $.dialog.errorTips('操作失败，请重试！');
            }).error(function () {
                $.dialog.errorTips('网络出错！');
            });
        }, 'selleradmin', true, exceptProducts, '#productSelectorBtns');
    }).on('click', '#selectAll', function () {
        //商品选择对话框批量选择按钮
        $.productSelector.selectAll();
    });
});

function deleteProduct(ids) {
    $.dialog.confirm('您确定要删除这些商品吗？', function () {
        var loading = showLoading();
        $.post('Delete', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除商品成功');
                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('删除商品失败!' + result.msg);
        });
    });
}

function bindSearchBtnClick() {

    $('#searchButton').click(function (e) {
        searchClose(e);
        search();
    });
}

function bindTabSwich() {
    $('div[type="filter"]').hide();
    $('.nav-tabs-custom li').click(function (e) {
        var _t = $(this);
        searchClose(e);
        clearFilter();
        _t.addClass('active').siblings().removeClass('active');
        var params = {};
        var type = _t.attr('type');
        curType = type;
        //初始数据
        params.saleStatus = '';
        params.auditStatus = [];
        params.keyWords = '';
        params.brandName = '';
        params.categoryId = '';
        params.productCode = '';
        params.freightTemplateId = '';
        params.shopName = '';
        params.startDate = '';
        params.endDate = '';
        params.overSafeStock = fgstock.checked;

        //补充条件
        var girdType = 'normal';
        switch (type) {
            case 'onSale':
                params.saleStatus = _t.attr('value');
                params.auditStatus = [2];
                normalFilter();
                break;
            case 'inStock':
                params.saleStatus = _t.attr('value');
                normalFilter();
                break;
            case 'unAudit':
                params.auditStatus = [1, 3];
                params.saleStatus = 1;
                girdType = 'audit';
                auditFilter();
                break;
            case 'saleOff':
                girdType = 'infractionSaleOff';
                params.auditStatus = [_t.attr('value')];
                saleOffFilter();
                break;
            case 'inDraft':
                params.saleStatus = _t.attr('value');
                normalFilter();
                break;

        }
        _auditStatus = params.auditStatus;
        _saleStatus = params.saleStatus;
        switch (girdType) {
            case 'normal':
                if (lastType == girdType)
                    $("#list").MallDatagrid('reload', params);
                else
                    initGrid(params);
                break;
            case 'audit':
                if (lastType == girdType) {
                    $("#list").MallDatagrid('reload', params);
                }
                else
                    initAuditGrid();
                break;
            case 'infractionSaleOff':
                if (lastType == girdType)
                    $("#list").MallDatagrid('reload', params);
                else
                    initInfractionSaleOffGrid();
                break;
        }

    });

}

function normalFilter() {
    $('.search-box').removeClass('only-line');
    $('div[saleOff]').hide();
    $('div[audit]').hide();
    $('div[normal]').show();
    var submit = $('form.custom-inline #searchButton');
    submit.prependTo($('div.submit'));

}

function auditFilter() {
    $('.search-box').addClass('only-line');
    $('div[saleOff]').hide();
    $('div[normal]').hide();
    $('div[audit]').show();
    var submit = $('div.submit #searchButton');
    submit.appendTo(submit.parent().parent());
}

function saleOffFilter() {
    $('.search-box').addClass('only-line');
    $('div[normal]').hide();
    $('div[audit]').hide();
    $('div[saleOff]').show();
    var submit = $('div.submit #searchButton');
    submit.appendTo(submit.parent().parent());
}

function clearFilter() {
    //$('#brandBox').val('');
    //$('#searchBox').val('');
    //$('#productCode').val('');
    //$('.start_datetime').val('');
    //$('.end_datetime').val('');
    $(".search-box form")[0].reset();
    $('.fg-stock input:checkbox').get(0).checked = false;
    categoryId = '';
    freightTemplateId = '';
    //$('#category1,#category2,#category3').MallLinkage('reset');
}

function initBrandAutoComplete() {

    //autocomplete
    $('#brandBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("../brand/getBrands", { "keyWords": $('#brandBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            if (item.envalue != null) {
                return item.value + "(" + item.envalue + ")";
            }
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });

}

function initInfractionSaleOffGrid() {
    lastType = 'infractionSaleOff';
    $("#list").html('');

    //商品表格
    $("#list").MallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的商品',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: [4], overSafeStock: fgstock.checked },
        operationButtons: "#saleOff",
        columns:
        [[
             { checkbox: true, width: 39 },
            {
                field: "Name", title: '商品', width: 450, align: 'left',
                formatter: function (value, row, index) {
                	var html = '';
                	if (row.ProductType == 1) {
                        html =  '<img class="pull-right" src="/Images/virtual.png"/>';
                    }
                    html += '<img style="margin-left:15px;" width="40" height="40" src="' + row.Image + '" /><span class="overflow-ellipsis" style="width:280px"><a title="' + value + '" target="_blank" href="/product/detail/' + row.Id + '">' + value + '</a></span>';
                    
                    return html;
                }
            },
        {
            field: "AuditState", title: "违规下架理由", align: "center",
            formatter: function (value, row, index) {
                var text = row.AuditReason;
                return text;
            }
        },
        {
            field: "s", title: "操作", width: 200, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.Name + '"/>';
                if (row.IsOpenLadder)
                    html += '<input name="hidIsOpenLadder" type="hidden" value="' + row.Id + '"/>';
                if (!row.IsLimitTimeBuy) {
                    html += '<a class="good-check" href="edit/' + row.Id + '">编辑</a>';
                }
                html += '<a class="good-up">上架</a>';
                html += '<a class="good-del">删除</a></span>';
                return html;
            }
        }
        ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
        }
    });


}

function initAuditGrid() {
    lastType = 'audit';
    $("#list").html('');

    //商品表格
    $("#list").MallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的商品',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: [1, 3], saleStatus: 1, overSafeStock: fgstock.checked },
        operationButtons: "#saleOff",
        columns:
        [[
             { checkbox: true, width: 40 },
             { field: "IsLimitTimeBuy", hidden: true, width: 40 },
            {
                field: "Name", title: '商品', align: 'left', width: 400,
                formatter: function (value, row, index) {
                	var html = '';
                	if (row.ProductType == 1) {
                        html = '<img class="pull-right" src="/Images/virtual.png"/>';
                    }
                    html += '<img class="ml15 mr10" width="40" height="40" src="' + row.Image + '" /><a class="single-ellipsis w350 h40 lh40" title="' + value + '" href="/product/detail/' + row.Id + '" target="_blank">' + value + '</a>';
                    
                    return html;
                }
            },
        {
            field: "AuditState", title: "审核", width: 200, align: "center",
            formatter: function (value, row, index) {
                var text = '';
                if (row.AuditState == 1)
                    text = '等待审核';
                else if (row.AuditState == 3) {
                    text = '<label style="color:red">未通过</label><br />' + (row.AuditReason == "null" || row.AuditReason == null ? "" : row.AuditReason);
                }
                return text;
            }
        },
        {
            field: "s", title: "操作", width: 200, align: "center",
            formatter: function (value, row, index) {
                var html = "";
                html = '<span class="btn-a"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.Name + '"/>';
                if (row.IsOpenLadder)
                    html += '<input name="hidIsOpenLadder" type="hidden" value="' + row.Id + '"/>';
                if (!row.IsLimitTimeBuy) {
                    html += '<a class="good-check" href="edit/' + row.Id + '">编辑</a>';
                }// html += '<a class="good-check" onclick="onSale(' + row.Id + ',\'' + row.Name + '\')">上架</a>';
                html += '<a class="good-down">下架</a>';
                html += '<a class="good-del">删除</a></span>';
                return html;
            }
        }
        ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
            bindAssociateTemplateBtnClickEvent();
        }
    });
}

function initGrid(params) {
    lastType = 'normal';
    $("#list").html('');
    normalFilter();
    if (window.location.href.toLowerCase().indexOf('tar=indraft') > 0) {
        $('.nav-tabs-custom li').removeClass("active");
        $('.nav-tabs-custom li[type="inDraft"]').addClass("active");
        if (!params) {
            params = {};
            params.saleStatus = 3;
        }
    }
    if (params) {
        params.productType = $("#productType").val();
    }
    //商品表格
    $("#list").MallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的商品',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: params ? params : { saleStatus: 1, auditStatus: [2], overSafeStock: fgstock.checked },
        operationButtons: "#saleOff",
        columns:
        [[
            { checkbox: true, width: 40 },
            { field: "IsLimitTimeBuy", hidden: true, width: 40 },
            {
                field: "ShopDisplaySequence", sort: true, title: '序号', width: 50,
                formatter: function (value, row, index) {
                    return ' <input class="text-order" style="width:40px;margin:0;" type="text" value="' + value + '" productid="' + row.Id + '" />';
                }
            },
            {
                field: "Name", title: '商品', align: 'left', width: 220,
                formatter: function (value, row, index) {
                	var html = '';
                	if(row.ProductType==1){
                        html = '<img class="pull-right" src="/Images/virtual.png"/>';
                    }
                    html += '<img class="ml15" width="40" height="40" src="' + row.Image + '" /><a class="single-ellipsis w75 lh20" title="' + value + '" target="_blank" href="/product/detail/' + row.Id + '">' + value + '</a>';
                    html = html + '<p class="lh20" style="color:#ffa702;">￥' + row.Price.toFixed(2) + '</p>';
                    
                    return html;
                }
            },
            { 
            	field: "ProductCode", title: '商品货号', width: 90,
            	formatter: function (value, row, index) {
                    return '<span class="overflow-ellipsis" style="width:80px" title="' + value + '">' + value + '</span>';
                }
            },
        { field: "BrandName", title: "品牌", width: 90, align: "center" },
        { field: "CategoryName", title: "商家分类", width: 90, align: "center" },
        { field: "Stock", title: "库存", width: 80, align: "center" },
        {
            field: "MaxBuyCount", title: "限购数", width: 70, align: "center",
            formatter: function (value) {
                if (value == 0)
                    return '不限购';
                return value;
            }
        },
         { field: "SaleCount", title: "实际销量", width: 70, align: "center" },
         { field: "VirtualSaleCounts", title: "虚拟销量", width: 72, align: "center" },
         { field: "PublishTime", title: "发布时间", width: 90, align: "center" },
        {
            field: "s", title: "操作", width: 120, align: "center",
            formatter: function (value, row, index) {
                html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.Name + '"/>';
                if (row.IsOpenLadder)
                    html += '<input name="hidIsOpenLadder" type="hidden" value="' + row.Id + '"/>';
                if (!row.IsLimitTimeBuy) {
                    html += '<a class="good-check" href="edit/' + row.Id + '">编辑</a>';
                }
                var qzoneurl = "http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshare_onekey?url=";
                var sinaurl = "http://service.weibo.com/share/share.php?source=bookmark&url=";
                var currenturl = encodeURIComponent('http://' + window.location.host + '/product/detail/' + row.Id + '?uid=' + row.Uid);
                if (row.SaleState == 1) {
                    html += '<a class="good-down">下架</a><a class="good-del">删除</a><div class="good-share"><i style="color:#337ab7">分享</i>';
                    html += '<div class="share-box text-center"><a href="javascript:void(0);" onclick="window.open(\'' + qzoneurl + currenturl + '&pics=http://' + window.location.host + encodeURIComponent(row.Image) + '&title=' + row.Name + '\');return false;" title="分享到QQ空间"><img src="/Images/qzone.png" /></a>';
                    html += '<a href="javascript:void(0);" onclick="window.open(\'' + sinaurl + currenturl + '&title=' + row.Name + '&pic=http://' + window.location.host + row.Image + '\');return false;" title="分享到新浪微博"><img src="/Images/weibo.png"/></a>';
                    html += '<a href="javascript:void(0);" title="分享到微信" onclick="showQrCode(\'' + row.QrCode + '\')"><img src="/Images/wx.png"/></a><s></s></div></div>';
                }

                else { html += '<a class="good-up">上架</a><a class="good-del">删除</a>'; }
                if (row.SaleState == 2 || row.SaleState == 3) {
                    html += '<a class="recommend block2" href="javascript:" data-product-id="{0}" data-relation-products="{1}">推荐商品</a>'.format(row.Id, row.RelationProducts);
                } else {
                    html += '<a class="recommend" href="javascript:" data-product-id="{0}" data-relation-products="{1}">推荐商品</a>'.format(row.Id, row.RelationProducts);
                }
                if (row.IsOverSafeStock) {
                    html += '<span class="red font12 inline-block ml5 mt5"><i class="glyphicon glyphicon-exclamation-sign"></i> 库存已到警戒值</span></span>';
                }
                return html;
            }
        }
        ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
            bindAssociateTemplateBtnClickEvent();
        }
    });
    orderTextEventBind();
}

function orderTextEventBind() {
    var _order = 0;
    $('.container').on('focus', '.text-order', function () {
        _order = parseInt($(this).val());
    });
    $('.container').on('blur', '.text-order', function () {
        var id = $(this).attr("productid");
        if ($(this).hasClass('text-order')) {
            if (isNaN($(this).val()) || parseInt($(this).val()) < 0) {
                $.dialog.errorTips("您输入的序号不合法,此项只能是大于零的整数！");
                $(this).val(_order);
            } else {
                if (parseInt($(this).val()) === _order) return;

                //更新商家商品序号
                var loading = showLoading();
                ajaxRequest({
                    type: 'POST',
                    url: "/selleradmin/product/updateShopDisplaySequence",
                    param: { id: id, order: parseInt($(this).val()) },
                    dataType: "json",
                    success: function (data) {
                        loading.close();
                        if (data.success == true) {
                            $.dialog.tips('更新成功！');
                            var pageNo = $("#list").MallDatagrid('options').pageNumber;
                            reload(pageNo);
                        }
                        else {
                            $.dialog.errorTips(data.msg, function () { location.reload(); });
                        }
                    }
                });
                //更新商家商品序号

            }
        }
    });
}

function initCategoryLinkage() {

    //$('#category1,#category2').MallLinkage({
    //    url: '../Category/getCategory',
    //    enableDefaultItem: true,
    //    defaultItemsText: '全部',
    //    onChange: function (level, value, text) {
    //        categoryId = value;
    //    }
    //});
    $("#category1").change(function () {
        if ($(this).val() == '0') {
            categoryId = null;
        }
        else {
            categoryId = $(this).val();
        }
    });

    $("#freightTemplates").change(function () {
        freightTemplateId = $(this).val();
    });

}

function search() {
    var brandName = $.trim($('#brandBox').val());
    var keyWords = $.trim($('#searchBox').val());
    var productCode = $.trim($('#productCode').val());
    var shopName = $.trim($('#shopName').val());
    var overSafeStock = $('.fg-stock input:checkbox').get(0).checked;
    var auditStatus = _auditStatus;
    var saleStatus = _saleStatus;
    if ($('.nav-tabs-custom li[type=unAudit]').hasClass('active')) {
        var auditState = $('select[name="auditState"]').val();
        if (!auditState) {
            auditStatus = [1, 3];
            saleStatus = 1;
        } else
            auditStatus = [auditState];
    }
    var startDate = $('.start_datetime').val();
    var endDate = $('.end_datetime').val();
    var productType = $("#productType").val();
    $("#list").MallDatagrid('reload', {
        BrandName: brandName, keyWords: keyWords, overSafeStock: overSafeStock,
        ShopCategoryId: categoryId, productCode: productCode, shopName: shopName,
        startDate: startDate, endDate: endDate, saleStatus: saleStatus, auditStatus: auditStatus, freightTemplateId: freightTemplateId, productType:productType
    });
}

function reload(pageNo) {

    $("#list").MallDatagrid('reload', { pageNumber: pageNo });
}

function getSelectedIds() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.Id);
    });
    return ids;
}
function setOverSafeStock(ids) {

    $.dialog({
        title: '警戒库存',
        lock: true,
        id: 'setOverSafeStock',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline" for="">警戒库存</label><input id="inputOverSafeStock" type="input" class="form-control input-sm"></select>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var close = false;
            var inputOverSafeStock = $('#inputOverSafeStock').val();
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: "SetProductOverSafeStock",
                data: { ids: ids.join(',').toString(), stock: inputOverSafeStock },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        $.dialog.succeedTips('设置成功!');
                        clearGridSelect();
                        close = true;
                    }
                    else
                        $.dialog.alert('设置异常!' + result.msg);
                },
                error: function () {
                    loading.close();
                    $.dialog.alert('设置出错！');
                }
            });
            return close;

        }
    })
}
function batchSettingFreightTemplate(ids) {
    $.ajax({
        type: "GET",
        url: "GetFreightTemplatesByIds",
        data: { ids: ids.join(',').toString() },
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.success == false) {
                $.dialog.errorTips(data.msg);
            } else {
                var databox = $("#selectFreightTemplate");
                databox.empty();
                databox.append("<option value='0'>请选择运费模板</option>");
                if (data.model != null) {
                    if (data.model && data.model.length > 0) {
                        $.each(data.model, function (i, model) {
                            var str = '';
                            if (model.ValuationMethod == 0) {
                                str = '【按件数】';
                            } else if (model.ValuationMethod == 1) {
                                str = '【按重量】';
                            } else if (model.ValuationMethod == 2) {
                                str = '【按体积】';
                            }
                            databox.append("<option value=\"" + model.Id + "\">" + model.Name + str + "</option>");
                        });
                    }
                }
            }
        },
        error: function () {
            $.dialog.errorTips("系统繁忙，请刷新重试");
        }
    });
    $.dialog({
        title: '批量设置运费模板',
        lock: true,
        id: 'batchSettingFreightTemplate',
        content: document.getElementById("batchSettingFreightTemplate"),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
            var close = false;
            var freightTemplateId = $('#selectFreightTemplate').val();
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: "batchSettingFreightTemplate",
                data: { ids: ids.join(',').toString(), freightTemplateId: freightTemplateId },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        if (result.failureNum <= 0) {
                            $.dialog.succeedTips('设置成功!');
                        } else {
                            $.dialog.succeedTips('' + result.successNum + '件实物商品设置成功，有' + result.failureNum + '件虚拟商品不支持运费设置');
                        }
                        clearGridSelect();
                        close = true;
                    }
                    else
                        $.dialog.errorTips(result.msg);
                },
                error: function () {
                    loading.close();
                    $.dialog.errorTips('设置出错！');
                }
            });
            return close;

        }
    })
}
function batchSettingStock(ids) {
    //$("input[type='text']").val('');
    $("input[type='text'][id='sourceStock']").val('');
    $("input[type='text'][id='updateStock']").val('');
    $("#btnSaveSourceStock").unbind('click').bind("click", function () {
        var newStock = Number($.trim($('#sourceStock').val()));
        if (newStock < 0) {
            $.dialog.errorTips('请输入有效库存！'); return;
        }
        $(".j_inputStock").val(newStock);
    });
    $("#btnSaveNewStock").unbind('click').bind("click", function () {
        var updateStock = Number($.trim($('#updateStock').val()));
        $(".j_inputStock").each(function () {
            if (Number($(this).val()) > 0 || updateStock > 0) {
                if ((Number($(this).val()) + updateStock) <= 0) {
                    $(this).val(0);
                } else {
                    $(this).val(Number($(this).val()) + updateStock);
                }
            }
        });
    });
    $.ajax({
        type: "GET",
        url: "GetProductsByIds",
        data: { ids: ids.join(',').toString() },
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.success == false) {
                $.dialog.errorTips("获取商品数据错误！");
            } else {
                var databox = $(".j_productResult");
                databox.empty();
                if (data.model != null) {
                    if (data.model && data.model.length > 0) {
                        $.each(data.model, function (i, model) {
                            databox.append('<tr><td field="ProductCode"><div style="text-align:left;"><span class="overflow-ellipsis" style="width:100px;">' + model.Sku + '</span></div></td><td field="ProductName" style="width:650px;"><div style="text-align:center;" class="MallDatagrid-cell ">' + model.ProductName + '</div></td><td field="shopName" style="width:80px;"><div style="text-align:center;" class="MallDatagrid-cell "><input class="form-control input-ssm input-number j_inputStock" type="text" id="sourceStock" value="' + model.Stock + '"  maxlength="6" style="width: 80px;"  skuAutoId="' + model.AutoId + '"/></div></td></tr>');
                        });
                    }
                }
                $(".j_inputStock").bind("keyup", function () {
                    $(this).val($(this).val().replace(/[^0-9]/g, ''));
                });
            }
        },
        error: function () {
            $.dialog.errorTips("系统繁忙，请刷新重试");
        }
    });
    $.dialog({
        title: '调整库存',
        lock: true,
        id: 'batchSettingStock',
        content: document.getElementById("batchSettingStock"),
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var close = false;
            var loading = showLoading();
            var stocks = [];
            $(".j_inputStock").each(function () {
                var info = new Object();
                info.AutoId = Number($(this).attr("skuautoid"));
                info.Stock = Number($(this).val());
                stocks.push(info);
            });
            $.ajax({
                type: "post",
                url: "batchSettingStock",
                data: { stocks: stocks, productIds: ids.join(',').toString() },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        $.dialog.succeedTips('设置成功');
                        var pageNo = $("#list").MallDatagrid('options').pageNumber;
                        reload(pageNo);
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
            return close;
        }
    });
}
function batchSettingPrice(ids) {
    //$("input[type='text']").val('');
    $("input[type='text'][id='price1']").val('');
    $("input[type='text'][id='price2']").val('');
    $("#btnSavePrice1").unbind('click').bind("click", function () {
        var price = Number($.trim($('#price1').val()));
        var type = $("#select1").val();
        if (type == 1) {//商城
            if (price <= 0) {
                $.dialog.errorTips('修改失败，改后的商品价格不能出现0或负数！'); return;
            }
            $(".j_SalePrice").val(price.toFixed(2));
        } else {//市场
            if (price < 0) {
                $.dialog.errorTips('请输入有效价格！'); return;
            }
            $(".j_MarketPrice").val(price.toFixed(2));
        }
    });
    $("#btnSavePrice2").unbind('click').bind("click", function () {
        var price = Number($.trim($('#price2').val()));
        var type = $("#select2").val();
        var select3 = $("#select3").val();//1=商城价，2=市场价
        if (price < 0 && type == 2) {
            $.dialog.errorTips('请输入有效价格！'); return;
        }
        if (type == 1 && price <= 0) {
            $.dialog.errorTips('请输入有效价格！'); return;
        }
        var select4 = $("#select4").val();//1+,2-
        if (type == 1) {
            var pass = true;
            $(".j_SalePrice").each(function () {
                if (!pass) {
                    return;
                }
                var priceTemp;
                if (select3 == 1) {
                    priceTemp = parseFloat($(this).val());
                } else {
                    var productId = $(this).attr("productid");
                    var j_MarketPrice = $("#j_MarketPrice_" + productId + "_1").val();
                    priceTemp = parseFloat(j_MarketPrice);
                }
                if (select4 == 1) {
                    $(this).val((priceTemp + price).toFixed(2));
                } else {
                    if (priceTemp - price <= 0) {
                        $(this).focus();
                        $.dialog.errorTips('修改失败，改后的商品价格不能出现0或负数！'); pass = false; return;
                        //$(this).val(0);
                    } else {
                        $(this).val((priceTemp - price).toFixed(2));
                    }
                }
            });
            if (!pass) {
                return false;
            }
        } else {
            var pass = true;
            $(".j_MarketPrice[isfirst=1]").each(function () {
                if (!pass) {
                    return;
                }
                var priceTemp;
                if (select3 == 1) {
                    priceTemp = parseFloat($(this).parents("tr").find(".j_SalePrice").val());
                } else {
                    var productId = $(this).attr("productid");
                    var j_MarketPrice = $("#j_MarketPrice_" + productId + "_1").val();
                    priceTemp = parseFloat(j_MarketPrice);
                }
                if (select4 == 1) {
                    $(this).val((priceTemp + price).toFixed(2));
                } else {
                    if (priceTemp - price < 0) {
                        $(this).focus();
                        $.dialog.errorTips('修改失败，改后的商品价格不能出现负数！'); pass = false; return;
                    } else {
                        $(this).val((priceTemp - price).toFixed(2));
                    }
                }
            });
            if (!pass) {
                return false;
            }
        }
    });
    $.ajax({
        type: "GET",
        url: "GetProductsByIds",
        data: { ids: ids.join(',').toString() },
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.success == false) {
                $.dialog.errorTips("获取商品数据错误！");
            } else {
                var databox = $(".j_productResult2");
                databox.empty();
                if (data.model != null) {
                    if (data.model && data.model.length > 0) {
                        $.each(data.model, function (i, model) {
                            var display = model.IsFirst == 1 ? 'block' : 'none';
                            databox.append('<tr><td field="ProductCode"><div style="text-align:left;"><span class="overflow-ellipsis" style="width:100px;">' + model.Sku + '</span></div></td><td field="ProductName" style="width:650px;"><div style="text-align:center;" class="MallDatagrid-cell ">' + model.ProductName + '</div></td><td field="shopName" style="width:80px;" ><div style="text-align:center;" class="MallDatagrid-cell "><input class="form-control input-ssm input-number j_MarketPrice" type="text" productid="' + model.ProductId + '" id="j_MarketPrice_' + model.ProductId + '_' + model.IsFirst + '" value="' + model.MarketPrice.toFixed(2) + '"   style="width: 80px;display:' + display + '"  SalePrice="' + model.SalePrice + '" IsFirst="' + model.IsFirst + '"  skuAutoId="' + model.AutoId + '"/></div></td><td field="shopName" style="width:80px;"><div style="text-align:center;" class="MallDatagrid-cell "><input class="form-control input-ssm input-number j_SalePrice" type="text" id="j_SalePrice" value="' + model.SalePrice.toFixed(2) + '"   style="width: 80px;"  productid="' + model.ProductId + '"  MarketPrice="' + model.MarketPrice + '"  HasSKU="' + model.HasSKU + '"  skuAutoId="' + model.AutoId + '"/></div></td></tr>');
                        });
                    }
                }
                $(".j_SalePrice,.j_MarketPrice").bind("keyup", function () {
                    $(this).val(($(this).val().match(/\d+(\.\d{0,2})?/) || [''])[0]);
                });
            }
        },
        error: function () {
            $.dialog.errorTips("系统繁忙，请刷新重试");
        }
    });
    $.dialog({
        title: '调整商城/市场价   &nbsp;&nbsp;(阶梯价商品不支持批量设置价格，已经过滤掉)',
        lock: true,
        id: 'batchSettingPrice',
        content: document.getElementById("batchSettingPrice"),
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var close = false;
            var pass = true;
            var products = [];
            var prices = [];
            $(".j_SalePrice").each(function () {
                var info = new Object();
                info.AutoId = Number($(this).attr("skuautoid"));
                info.SalePrice = parseFloat($(this).val());
                info.HasSku = $(this).attr("hassku");
                info.ProductId = Number($(this).attr("productid"));
                if (info.SalePrice <= 0) {
                    pass = false;
                    $(this).focus();
                    $.dialog.errorTips("修改失败，改后的商品价格不能出现0或负数！");
                    return;
                }
                prices.push(info);
            });
            $(".j_MarketPrice[isfirst=1]").each(function () {
                var info = new Object();
                info.Id = Number($(this).attr("productid"));
                info.MarketPrice = parseFloat($(this).val());
                if (info.MarketPrice < 0) {
                    pass = false;
                    $(this).focus();
                    $.dialog.errorTips("请输入有效价格！");
                    return;
                }
                products.push(info);
            });
            if (!pass) {
                return false;
            }
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: "batchSettingPrice",
                data: { products: products, prices: prices },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        $.dialog.succeedTips('设置成功');
                        var pageNo = $("#list").MallDatagrid('options').pageNumber;
                        reload(pageNo);
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
            return close;
        }
    });
}
function saleOff(ids) {
    $.dialog.confirm('您确定要下架这些商品吗？', function () {
        var loading = showLoading();
        $.post('batchSaleOff', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('下架商品成功');
                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('下架商品失败!' + result.msg);
        });
    });
}

function onSale(ids) {
    $.dialog.confirm('您确定要上架这些商品吗？', function () {
        var loading = showLoading();
        $.post('batchOnSale', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('上架商品成功');
                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('上架商品失败!' + result.msg);
        });
    });
}

function initBatchBtnShow() {
    var type = $('.nav-tabs-custom li.active').attr('type');
    if (type == 'onSale') {//当前为销售中tab
        $('#batchSaleOff')
            .show()
            .unbind('click')
            .click(function () {
                var ids = getSelectedIds();
                if (ids.length > 0)
                    saleOff(ids);
                else
                    $.dialog.tips('请至少选择一件商品');
            });

    }
    else if (type == 'inStock' || type == 'saleOff' || type == "inDraft") {
        $('#batchOnSale')
           .show()
           .unbind('click')
           .click(function () {
               var ids = getSelectedIds();
               if (ids.length > 0)
                   onSale(ids);
               else
                   $.dialog.tips('请至少选择一件商品');
           });
    }

    if (type == 'unAudit' || type == 'saleOff')
        $('#associateTemplate').hide();
    else
        $('#associateTemplate').show();
    $('#batchDelete')
        .unbind('click')
        .click(function () {
            var ids = getSelectedIds();
            if (ids.length > 0)
                deleteProduct(ids);
            else
                $.dialog.tips('请至少选择一件商品');
        });
    $('#overSafeStock').unbind('click').click(function (e) {
        var ids = getSelectedIds();
        if (ids.length == 0) {
            $.dialog.alert('请选择要设置的商品！');
            return;
        }
        setOverSafeStock(ids);
    });
    $('#batchSetting').on("change", function () {
        var type = $(this).val();
        if (type > 0) {
            var ids = getSelectedIds();
            if (ids.length == 0) {
                $.dialog.alert('请选择要设置的商品！');
                $(this).val('0');
                return;
            }
            if (type == 1) {
                batchSettingFreightTemplate(ids);//批量设置运费模板
            } else if (type == 2) {
                batchSettingStock(ids);//批量调整库存
            } else if (type == 3) {
                //当前列表是商城价的商品id，然后选中的商品id过滤已是阶梯价的商品Id
                var hidIsOpenLadderIds = [];
                $('input[name="hidIsOpenLadder"]').each(function () {
                    hidIsOpenLadderIds.push($(this).val());
                });

                if (hidIsOpenLadderIds != null && hidIsOpenLadderIds.length > 0) {
                    var newids=[];
                    for (var inum = 0; inum < ids.length; inum++) {
                        var aid = ids[inum];//过滤阶梯价商品Id
                        if (hidIsOpenLadderIds.indexOf(aid) == -1)
                            newids.push(aid);
                    }
                    if (newids.length <= 0) {
                        $.dialog.tips('选择的商品是阶梯价商品不能批量改价，请至少选择另外一件商品');
                    }
                    ids = newids;
                }
                if (ids.length > 0)
                    batchSettingPrice(ids);//批量调整价格
            }
        }
        $(this).val('0');
    });
}

function initDatePicker() {

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});
    //$(".end_datetime").click(function () {
    //    $('.start_datetime').datetimepicker('show');
    //});

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

}

function bindTemplate(ids) {

    $.dialog({
        title: '关联版式',
        lock: true,
        id: 'addArticleSort',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline" for="">顶部</label><select id="top" class="form-control input-sm"></select>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline" for="">底部</label>',
                '<select class="form-control input-sm" id="bottom"></select>',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var close = false;
            var topTemplateId = $('#top').val();
            var bottomTemplateId = $('#bottom').val();
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: "bindTemplates",
                data: { topTemplateId: topTemplateId, bottomTemplateId: bottomTemplateId, ids: ids.toString() },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        $.dialog.tips('关联成功');
                        clearGridSelect();
                        close = true;
                    }
                    else
                        $.dialog.alert('关联失败!' + result.msg);
                },
                error: function () {
                    loading.close();
                    $.dialog.alert('关联失败:请求出错');
                }
            });
            return close;

        }
    });

    $.post('../ProductDescriptionTemplate/getAll', {}, function (result) {
        var top = $('#top');
        var bottom = $('#bottom');
        top.append('<option value="0">不选择</option>');
        bottom.append('<option value="0">不选择</option>');
        $.each(result, function () {
            if (this.Position == 1)
                top.append('<option value="' + this.Id + '">' + this.Name + '</option>');
            else
                bottom.append('<option value="' + this.Id + '">' + this.Name + '</option>');
        });

    });

}

function clearGridSelect() {
    $("#list").MallDatagrid('clearSelections');
    $(".check-all").prop("checked",false);
}

function bindAssociateTemplateBtnClickEvent() {

    $('#associateTemplate').click(function () {

        var selectedIds = getSelectedIds();
        var ids = getSelectedIds();
        if (ids.length > 0)
            bindTemplate(ids);
        else
            $.dialog.tips('请至少选择一件商品');
    });
}

function ExportExecl() {
    var type = $('.nav-tabs-custom li[class="active"]').attr("type");
    var selecttypevalue = $('.nav-tabs-custom li[class="active"]').attr("value");
    var saleStatus = "";
    var auditStatus = "";
    var auditStatuses = "";

    var brandName = $.trim($('#brandBox').val());
    var keyWords = $.trim($('#searchBox').val());
    var productCode = $.trim($('#productCode').val());
    var shopName = $.trim($('#shopName').val());
    var auditStatuses = "";
    if ($('.nav-tabs-custom li.active').attr('type') == 'unAudit') {
        auditStatuses = $('select[name="auditState"]').val();
        if (!auditStatuses) {
            auditStatuses = '1,3';
        }
    }
    var startDate = $('.start_datetime').val();
    var endDate = $('.end_datetime').val();
    switch (type) {
        case 'onSale':
            saleStatus = selecttypevalue;
            auditStatus = 2;
            break;
        case 'inStock':
            saleStatus = selecttypevalue;
            break;
        case 'unAudit':
            auditStatus = "";
            saleStatus = "";
            auditStatuses = "1,3";
            break;
        case 'saleOff':
            auditStatus = selecttypevalue;
            break;
        case 'inDraft':
            saleStatus = selecttypevalue;
            break;
    }
    var productType = $("#productType").val();
    var href = "/SellerAdmin/Product/ExportToExcel?";
    href += "brandName=" + brandName + "&keyWords=" + keyWords + "&categoryId=" + categoryId + "&productCode=" + productCode + "&shopName=" + shopName + "&startDate=" + startDate + "&endDate=" + endDate + "&auditStatuses=" + auditStatuses + "&auditStatus=" + auditStatus + "&saleStatus=" + saleStatus + "&freightTemplateId=" + freightTemplateId + "&productType=" + productType;
    $("#aExport").attr("href", href);
}