var platFormCategoryId;
var categoryId;
var brandid;
var paraSaleStatus = -1;
var MaxFileSize = 31457280;//30M
var MaxImportCount = 20;
var RefreshInterval = 2;//Second
var _refreshProcess;//定时器
var loading;
var sellercate1;
var sellercate2;
var radialObj;
var dialog;
var isend = 0;
var guid = "";

$(function () {
    $("#btnFile").bind("change", function () {
        if ($("#btnFile").val() != '') {
            var dom_btnFile = document.getElementById('btnFile');
            if (typeof (dom_btnFile.files) == 'undefined') {
                try {
                    var fso = new ActiveXObject('Scripting.FileSystemObject');
                    var f = fso.GetFile($("#btnFile").val());
                    if (f.Size > MaxFileSize) {
                        $.dialog.tips('选择的文件太大');
                        return;
                    }
                }
                catch (e) {
                    errorTips(e);
                }
            }
            else {
                if (dom_btnFile.files.length > 0 && dom_btnFile.files[0].size > MaxFileSize) {
                    $.dialog.tips('选择的文件太大');
                    return;
                }
            }
            var filename = $("#btnFile").val().substring($("#btnFile").val().lastIndexOf('\\') + 1);
            $('#inputFile').val(filename);
        }
        else {
            $('#inputFile').val('请选择文件');
        }
    });
    $('#btnUpload').bind('click', function () {
        if ($("#inputCanCreate").val() == 0 || $("#inputCanCreate").val() == "0") {
            $.dialog.tips($("#inputCanNotCreateMessage").val());
            return;
        }
        if ($('#category3').val() == null || $('#category3').val() == '') {
            $.dialog.tips('请选择一个子级类目');
            return;
        }
        sellercate1 = $('#sellercategory1').val() || '0';
        sellercate2 = $('#sellercategory2').val() || '0';

        if ((sellercate1 == '' || sellercate1 == '0') && (sellercate2 == '' || sellercate2=='0')) {
            $.dialog.tips('请选择一个商品分类');
            return;
        }
        paraSaleStatus = -1;
        $('input[name=paraSaleStatus]').each(function (idx, el) {
            if ($(el).attr('checked') == 'checked') {
                paraSaleStatus = $(el).attr('status');
            }
        });
        if (paraSaleStatus == -1) {
            $.dialog.tips('请选择一个商品状态');
            return;
        }
        if (parseInt($('#freightTemplate').attr('value')) <= 0) {
            $.dialog.tips('请选择一个运费模板！');
            return;
        }
        var filename = $('#inputFile').val();
        if (filename == '请选择文件') {
            $.dialog.tips('请选择文件');
            return;
        }
        else {
            GetImportOpCount();
        }
    });

    $('#category1,#category2,#category3').MallLinkage({
        url: 'GetPlatFormCategory',
        enableDefaultItem: true,
        defaultItemsText: '',
        onChange: function (level, value, text) {
            platFormCategoryId = value;
            fnGetBrandByCategory(value);
        }
    });

    getCategory();

    //初始导入状态
    var count = $('#inputCount').val();
    var total = $('#inputTotal').val();
    var success = $('#inputSuccess').val();
    if (count < total) {
        SetImportProcess();
        ShowImportProcess();
    }
    $('#btnAddTemplate').bind('click', function () {
        window.open('/SellerAdmin/FreightTemplate/Add?displayUrl=/SellerAdmin/FreightTemplate/Index&tar=freighttemplate');
    });
    $('#btnAddSellerCategory').bind('click', function () {
        window.open('/SellerAdmin/category/management?&tar=sellercategory');
    });
    $('.j_addUrl').click(function () {
        if ($("div.j_link").length == 5) {
            $.dialog.errorTips('最多一次添加5个网址');
            return;
        }
        var last = $("div.j_link:last-child");//每次新增获取最后一个文本框，附加
        if (last.length == 0) {
            last = $("div.j_link:first-child")
        }
        var cloneInfo = last.clone(true);
        cloneInfo.find(".j_linkurl").val("");
        last.after(cloneInfo);
        cloneInfo.find("a.del-url").show();
        //last.find("a.del-url").remove();
        if ($("div.j_link").length == 5) {
            $('.j_addUrl').hide();
        }
    });
    initDel();
    $('#btnSpider').click(function () {
        radialObj.value(0);
        isend = 0;//每次提交则重新赋值为0
        var bigCategoryId = $("#category1").val();//大类
        var midCategoryId = $("#category2").val();//中类
        var smallCategoryId = $("#category3").val();//小类
        var sellerBigCategoryId = $("#sellercategory1").val();//商家分类
        var sellerMidcategoryId = $("#sellercategory2").val();//商家分类
        var brandId = $("#selectBrand").val();//品牌
        var freightTemplateId = $("#freightTemplate").val();//运费模板
        if (smallCategoryId <= 0) {
            $.dialog.errorTips('请选择完整平台分类');
            return;
        }
        if (sellerBigCategoryId <= 0) {
            $.dialog.errorTips('请选择商家分类');
            return;
        }
        if (freightTemplateId <= 0) {
            $.dialog.errorTips('请选择运费模板');
            return;
        }
        var isPass = true;
        $("div.j_link").each(function () {
            var currentObj = $(this).find(".j_linkurl");
            if ($.trim(currentObj.val()).length == 0) {
                $.dialog.errorTips('请填写抓取地址');
                currentObj.focus();
                isPass = false;
                return;
            } else {
                var result = checkUrl($.trim(currentObj.val()));
                if (!result) {
                    $.dialog.errorTips('请填写正确的URL地址');
                    currentObj.focus();
                    isPass = false;
                    return;
                }
            }
        });
        if (!isPass) {
            return;
        }
        var submitData = {}; //初始化数据对象
        submitData.BigCategoryId = bigCategoryId;
        submitData.MidCategoryId = midCategoryId;
        submitData.SmallCategoryId = smallCategoryId;
        submitData.SellerBigCategoryId = sellerBigCategoryId;
        submitData.SellerMidcategoryId = sellerMidcategoryId;
        submitData.BrandId = brandId;
        submitData.FreightTemplateId = freightTemplateId;
        submitData.GrabUrl = [];//返点规则
        var urls = [];
        $("div.j_link").each(function () {
            var linkurl = $(this).find(".j_linkurl").val();//获取抓取地址
            urls.push(linkurl);
        });
        submitData.GrabUrl = urls;

        try {
            var objectString = JSON.stringify(submitData);
            var loading = showLoading();
            submitData.Guid = guid;
            $.post('/selleradmin/ProductImport/SaveProducts', submitData, function (result) {
                loading.close();
                if (result.success) {
                    //$.dialog.succeedTips('保存成功', function () {
                    //});
                    dialog = $.dialog({
                        title: '正在导入',
                        lock: true,
                        padding: '0 40px',
                        width: 480,
                        id: 'spiderResult',
                        content: $('#spiderResult')[0]
                    });
                    $(".j_total").html(result.total);
                    guid = result.guid;
                    refreshPercent(result.guid);//上传后开始刷新百分比
                }
                else {
                    if (result.msg.indexOf('上一批') > -1) {
                        $.dialog.confirm('上一批数据正在导入中，确认终止上次导入？', function () {
                            var id = result.guid;
                            $.post('/selleradmin/ProductImport/CancleImport', { guid: id });
                            isend = 1;
                            radialObj.value(0);
                            guid = "";
                        });
                    } else {
                        $.dialog.errorTips('导入失败！' + result.msg);
                    }

                }
            }, "json");

        }
        catch (e) {
            $.dialog.errorTips(e.message);
        }

    });
    radialObj = $('#indicatorContainer').radialIndicator({
        displayNumber: true,
        barColor: '#0066FF',
        barWidth: 5,
        initValue: 0,
        percentage: true
    }).data('radialIndicator');
});
function checkUrl(url) {
    var expression = /http(s)?:\/\/([\w-]+\.)+[\w-]+(\/[\w- .\/?%&=]*)?/;
    var objExp = new RegExp(expression);
    if (objExp.test(url) != true) {
        return false;
    } else {
        return true;
    }
}
function initDel() {
    $(".del-url").click(function () {
        $(this).parent().remove();
        //var last = $("div.j_link:last-child");//移除后，获取最新的最后一个，但要排除掉第一个
        //if ($("div.j_link").length > 1) {
        //    last.append('<a class="del-url" style="margin-top:10px;display: inline-block; cursor: pointer;">删除</a>');
        //    initDel();
        //} else {
        //    if (last.find(".del-url").length == 0) {
        //        last.append('<a class="del-url" style="margin-top:10px;display: inline-block; cursor: pointer;display:none">删除</a>');
        //        initDel();
        //    }
        //}
        if ($("div.j_link").length < 5) {
            $('.j_addUrl').show();
        }
    });
}
function fnUploadFileCallBack(filename) {
    var shopCategory = parseFloat(sellercate1);
    if (sellercate2 != '' && sellercate2 != '0')
        shopCategory = parseFloat(sellercate2);
    $.ajax({
        type: 'get'
        , url: '../AsyncProductImport/ImportProduct'
        , data: { paraCategory: parseFloat($('#category3').val()), paraShopCategory: shopCategory, paraBrand: $('#selectBrand').val(), paraSaleStatus: paraSaleStatus, _shopid: $('#inputShopid').val(), _userid: $('#inputUserid').val(), freightId: $('#freightTemplate').val(), file: filename }
        , datatype: 'json'
        , success: function (data) {
            $('.ajax-loading').remove();
            $.dialog.alert(data.message, function () {
                window.history.go(0);
            }, 3);
        }
    });
}
function ShowImportProcess() {
    _refreshProcess = setInterval(SetImportProcess, RefreshInterval * 1000);
}
function SetImportProcess() {
    $.ajax({
        type: 'get'
       , url: 'GetImportCount'
       , datatype: 'json'
       , success: function (data) {
           if (data.Total > data.Count) {
               if ($('.ajax-loading').length == 0) {
                   loading = showLoading(data.Count + '/' + data.Total);
                   if ($('.ajax-loading').css('display') == 'none') {
                       $('.ajax-loading').show();
                   }
               }
               else {
                   if ($('.ajax-loading').css('display') == 'none') {
                       $('.ajax-loading').show();
                   }
                   $('.ajax-loading p').text(data.Count + '/' + data.Total);
               }
           }
           if (data.Success == 1) {
               clearInterval(_refreshProcess);//完成后清除定时器
               $('.ajax-loading').hide();
               $.dialog.succeedTips('导入完成');
           }
       }
    });
}

function GetImportOpCount() {
    $.ajax({
        type: 'get'
       , url: 'GetImportOpCount'
       , datatype: 'json'
       , success: function (data) {
           if (data.Count >= MaxImportCount) {
               $.dialog.tips('上传人数较多，请稍等。。。');
               return;
           }
           var dom_iframe = document.getElementById('iframe');
           //非IE、IE
           dom_iframe.onload = function () {
               var filename = this.contentDocument.body.innerHTML;
               if (filename != 'NoFile' && filename != 'Error') {
                   fnUploadFileCallBack(filename);//上传文件后，继续导入商品操作
                   $('#inputFile').val('请选择文件');
               }
               else {
                   $.dialog.tips('上传文件异常');
               }
               this.onload = null;
               this.onreadystatechange = null;
           };
           //IE
           dom_iframe.onreadystatechange = function () {
               if (this.readyState == 'complete' || this.readyState == 'loaded') {
                   var filename = this.contentDocument.body.innerHTML;
                   if (filename != 'NoFile' && filename != 'Error') {
                       fnUploadFileCallBack(filename);//上传文件后，继续导入商品操作
                   }
                   else {
                       loading.close();
                       $.dialog.tips('上传文件异常');
                   }
                   this.onload = null;
                   this.onreadystatechange = null;
               }
           };
           loading = showLoading('正在上传');
           $('#formUpload').submit();
       }
    });
}

function fnGetBrandByCategory(cateid) {
    $('#selectBrand').MallLinkage({
        url: 'GetShopBrand?categoryId=' + cateid,
        enableDefaultItem: true,
        defaultItemsText: '',
        onChange: function (level, value, text) {
            brandid = value;
        }
    });
}
function BindfreightTemplate() {
    $.ajax({
        type: 'get'
       , url: '/selleradmin/product/GetFreightTemplate'
       , datatype: 'json'
       , success: function (result) {
           if (result.success) {
               for (var i = 0 ; i < result.model.length; i++) {
                   var opt = $('#freightTemplate').find('option[value=' + result.model[i].Value + ']');
                   if (opt.length == 0) {
                       $('#freightTemplate').append('<option value="' + result.model[i].Value + '">' + result.model[i].Text + '</option>');
                   }
               }
           }
       }
    });
}
function BindSellerCategory() {
    $.ajax({
        type: 'post'
       , url: '/selleradmin/Category/getCategory'
       , datatype: 'json'
       , success: function (result) {
           if (result != null) {
               for (var i = 0 ; i < result.length; i++) {
                   var opt = $('#sellercategory1').find('option[value=' + result[i].Key + ']');
                   if (opt.length == 0) {
                       $('#sellercategory1').append('<option value="' + result[i].Key + '">' + result[i].Value + '</option>');
                   }
               }
               getCategory();
           }
       }
    });
}
function refreshPercent(guid) {
    if (isend == 1) {
        return;
    }
    $.post('/selleradmin/ProductImport/RefreshImportProduct', { guid: guid }, function (data) {
        radialObj.value(data.value);
        $(".j_current").html(data.index);
        if (data.success) {
            setTimeout(function () { dialog.close(); location.href = '/selleradmin/ProductImport/SpiderSuccess?guid=' + guid }, 2000);//延迟3秒再关闭
        } else {
            setTimeout(refreshPercent(guid), 3000);
        }
    });
}

function getCategory() {
    $('#sellercategory1,#sellercategory2').MallLinkage({
        url: '../Category/getCategory',
        enableDefaultItem: false,
        defaultItemsText: '',
        onChange: function (level, value, text) {
            categoryId = value;
        }
    });
}