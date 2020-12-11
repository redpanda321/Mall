window._refreshFooter = function () { };//刷新底部信息
window._floorSave = function () { };
$(function () {
    InitSelect();
    InitEditAdsImg();
    imageAdsClickEventBind();
    InitUpload();
    SetLogo();
    AddUrl();
    InitDel();
    InitUpDown();
    InitSelectDown();
});
function InitEditAdsImg() {
    var slideAdsCount = $("#slideAdsCount").val();
    if (slideAdsCount > 0) {
        $("div.j_adsPic").each(function (i) {
            var pic = $(this).attr("pic");
            $(".j_HandSlideAdsPic_" + i).MallUpload(
                  {
                      title: '显示图片',
                      imageDescript: '',
                      displayImgSrc: pic,
                      imgFieldName: "HandSlideAdsPic_" + i,
                      dataWidth: 5,
                      headerWidth: 4
                  });
        });
    }
}
function InitUpload() {
    $("#uploadImg").MallUpload(
   {
       displayImgSrc: logo,
       imgFieldName: "Logo",
       title: 'LOGO:',
       imageDescript: '160*160',
       dataWidth: 8
   });
    $(".j_HandSlideAdsPic").MallUpload(
                   {
                       title: '显示图片',
                       imageDescript: '',
                       displayImgSrc: '',
                       imgFieldName: "HandSlideAdsPic",
                       dataWidth: 8
                   });
}
function SetLogo() {
    $('.logo-area').click(function () {
        $.dialog({
            title: 'LOGO设置',
            lock: true,
            width: 350,
            id: 'logoArea',
            content: document.getElementById("logosetting"),
            padding: '10px 30px',
            okVal: '保存',
            ok: function () {
                var logosrc = $("input[name='Logo']").val();
                if (logosrc == "") {
                    $.dialog.tips("请上传一张LOGO图片！");
                    return false;
                }
                var loading = showLoading();
                $.post(setlogoUrl, { logo: logosrc },
                    function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.succeedTips("LOGO修改成功！");
                            $("input[name='Logo']").val(data.logo);
                            logo = data.logo;
                        }
                        else { $.dialog.errorTips("LOGO修改失败！") }
                    });
            }
        });
    });
}
function imageAdsClickEventBind() {
    //顶部广告栏，横条广告，3个推荐图
    $('a[type="imageAd"]').click(function () {
        var that = this;
        var thisPic = $(this).attr('pic');
        var thisUrl = $(this).attr('url');
        var title = $(this).attr('title');
        var istopAd = $(this).attr('istopAd');
        $(".j_openTopImageAd").hide();
        if (istopAd == 1) {
            $(".j_openTopImageAd").show();
        }
        $.dialog({
            title: title,
            lock: true,
            width: 480,
            padding: '10px 30px',
            id: 'goodsArea',
            content: document.getElementById("imageAd"),
            okVal: '保存',
            init: function () {
                $("#HandSlidePic").MallUpload(
                {
                    title: '上传图片',
                    imageDescript: $(that).attr("imageDescript"),
                    displayImgSrc: thisPic,
                    imgFieldName: "HandSlidePic",
                    dataWidth: 8
                });
                $("#url").val(thisUrl);
            },
            ok: function () {
                var valida = false;
                var id = parseInt($(that).attr('value'));
                var url = $("#url").val();
                var pic = $("#HandSlidePic").MallUpload('getImgSrc');
                if (url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }
                if (pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }
                var loading = showLoading();
                var openTopImageAd;
                if ($(".j_openTopImageAd").is(':visible')) {
                    openTopImageAd = $("input[name='openTopImageAd']:checked").val();
                }
                $.ajax({
                    type: "POST",
                    url: "UpdateImageAd",
                    data: { url: url, pic: pic, id: id, openTopImageAd: openTopImageAd },
                    dataType: "json",
                    async: false,
                    success: function (data) {
                        loading.close();
                        if (data.success) {
                            $(that).attr('pic', data.imageUrl);
                            $(that).attr('url', url);
                            $.dialog.tips('保存成功!');
                        }
                        else {
                            $.dialog.errorTips('保存失败！' + data.msg);
                            return false;
                        }
                    },
                    error: function (data) {
                        loading.close(); $.dialog.errorTips('操作失败,请稍候尝试.');
                    }
                });
            }
        });
    });

    //轮播图
    $('a[type="slideAds"]').click(function () {
        var that = this;
        var thisPic = $(this).attr('pic');
        var thisUrl = $(this).attr('url');
        $.dialog({
            title: '轮播图',
            lock: true,
            width: 480,
            padding: '15px 30px',
            id: 'slideAds',
            content: document.getElementById("slideAdsSetting"),
            okVal: '确定',
            init: function () {
            },
            ok: function () {
                //var valida = false;
                //var id = parseInt($(that).attr('value'));
                //var url = $("#url").val();
                //var pic = $("#HandSlidePic").MallUpload('getImgSrc');
                //if (url.length === 0) { $("#url").focus(); $.dialog.errorTips('链接地址不能为空.'); return valida; }
                //if (pic.length === 0) { $.dialog.errorTips('图片不能为空.'); return valida; }

                var slideads = [];
                var o = [];
                $(".j_slideads").each(function () {
                    var info = new Object();
                    info.Id = $(this).find("#adsId").val();
                    info.Pic = $(this).find(".hiddenImgSrc").val();
                    info.Url = $(this).find("input[type=text]").val();
                    o.push(info);
                });
                slideads = o;
                var loading = showLoading();
                $.ajax({
                    type: "POST",
                    url: "SaveSlideAds",
                    data: { slideads:slideads },
                        dataType: "json",
                        async: false,
                        success: function (data) {
                        loading.close();
                                if (data.success) {
                                    $.dialog.tips('保存成功!');
                                    location.reload();
                                }
                                else {
                                    $.dialog.errorTips('保存失败！' + data.msg);
                                    return false;
                                }
                            },
                    error: function (data) {
                        loading.close(); $.dialog.errorTips('操作失败,请稍候尝试.');
                    }
                });
            }
        });
    });

    //导航条
    $('a[type="navigation"]').click(function () {
        $.dialog({
            title: '导航条设置',
            lock: true,
            content: '<iframe id="iframeNavigation" src="/SellerAdmin/Navigation/management" style="border:none;width:1200px;height:450px;"></iframe>',
            padding: '0',
            ok: function () {
            },
            close: function () {
            }
        });
    });

    //楼层编辑
    $('a[type="shopHomeModule"]').click(function () {
        $.dialog({
            title: '楼层编辑',
            lock: true,
            content: '<iframe id="iframeCategory" src="/SellerAdmin/ShopHomeModule/management" style="border:none;width:1230px;height:450px;"></iframe>',
            padding: '0',
            ok: function () {
                _floorSave();
                $.dialog.succeedTips("保存成功");
            },
            close: function () {
            }
        });
    });

    //引导搜索设置
    $('a[type="category"]').click(function () {
        $.dialog({
            title: '引导搜索设置',
            lock: true,
            content: '<iframe id="iframeCategory" src="/SellerAdmin/PageSettings/category" style="border:none;width:1230px;height:450px;"></iframe>',
            padding: '0',
            ok: function () {
            },
            close: function () {
            }
        });
    });

    $('a[type="editFooter"]').click(function () {
        $.dialog({
            title: '页脚编辑',
            lock: true,
            content: '<iframe id="iframeShopHomeModule" src="/SellerAdmin/ShopHomeModule/EditFooter" style="border:none;width:800px;height:380px;"></iframe>',
            padding: '0',
            okVal: '保存',
            ok: function () {
                _refreshFooter();
                $.dialog.succeedTips("保存成功");
            },
            close: function () {
            }
        });
    });
}
function InitSelect(index) {
    var selectC = ".j_slideads li";
    if (typeof (index) != "undefined") {
        selectC = ".j_slideads:gt(" + index + ") li";
    }
    $(selectC).click(function () {
        var type = $(this).attr("type");
        var current = $(this).parent().parent().parent();
        if (type == 1) {
            var selectids = [];
            $.productSelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.id);
                });
                if (ids.length > 1) {
                    $.dialog.errorTips("只能选择一个商品");
                    return;;
                }
                current.find("input[type=text]").attr("value", '/product/detail/' + ids.join(","));
                current.find("input[type=text]").attr("disabled", true);
            }, "selleradmin", false);
        } else if (type == 2) {
            var selectids = [];
            $.couponSelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.game_id);
                });
                if (ids.length > 1) {
                    $.dialog.errorTips("只能选择一个优惠券");
                    return;;
                }
                current.find("input[type=text]").attr("disabled", true);
                current.find("input[type=text]").attr("value", '/product/detail/' + ids.join(","));
            }, null, false);
        }
        else if (type == 3) {
            var selectids = [];
            $.limitTimeBuySelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.Id);
                });
                if (ids.length > 1) {
                    $.dialog.errorTips("只能选择一个商品");
                    return;;
                }
                current.find("input[type=text]").attr("value", '/limittimebuy/detail/' + ids.join(","));
                current.find("input[type=text]").prop("disabled", true);
            }, null, false);
        } else {
            current.find("input[type=text]").val("/").removeAttr('disabled');
            
        }
    });
}
function AddUrl() {
    $(".j_addUrl").click(function () {
        if ($("li.j_slideads").length == 5) {
            $.dialog.errorTips('最多添加5张轮播图');
            return;
        }
        var last = $("li.j_slideads:last-child");
        if (last.length == 0) {
            last = $("li.j_slideads:first-child")
        }
        var index = $("li.j_slideads").length;
        var li = '<li class="item j_slideads"><div class="form-group upload-img clearfix"><div id="HandSlideAdsPic' + index + '"class="fl form-group upload-img clearfix j_HandSlideAdsPic' + index + '"></div><div class="fl"><span class="help-default">1920*520</span>'+
        ' <label style="margin-left:20px;"><a v-on:click="up(index)"class="glyphicon glyphicon-arrow-up"title="上移"></a></label>'+
        ' <label><a v-on:click="down(index)"class="glyphicon glyphicon-arrow-down"title="下移"></a></label>'+
        ' <label><a v-on:click="del(index)"title="删除"class="glyphicon glyphicon-remove"></a></label><input type="hidden" id="sort" name="sort" class="j_sort" value="0" /></div></div><div class="form-group upload-img clearfix"><label class="col-sm-2 control-label fl"for="">链接到：</label><input class="form-control input-sm"type="text"id="url"value="/"/> <div class="downlist j_selectDown"><span>请选择</span><ul><li type="1">选择商品</li><!--<li type="2">优惠券</li>--><li type="3">限时购商品</li><li type="4">自定义链接</li></ul></div></div></li>';
        last.after(li);
        $(".j_HandSlideAdsPic" + index).MallUpload(
               {
                   title: '显示图片',
                   imageDescript: '',
                   displayImgSrc: '',
                   imgFieldName: "HandSlideAdsPic" + index,
                   dataWidth: 5,
                   headerWidth: 4
               });
        InitSelect(index - 1);//新增一个元素后初始化选择链接下拉框
        InitDel(index - 1);
        InitUpDown(index - 1);
    });
}
function InitDel(index) {
    var remove = ".glyphicon-remove";
    if (typeof (index) != "undefined") {
        remove = ".j_slideads:gt(" + index + ") .glyphicon-remove";
    }

    $(remove).click(function () {
        if ($("li.j_slideads").length == 1) {
            $.dialog.errorTips('最少要有1张轮播图');
            return;
        }
        $(this).parents("li").remove();
    });
}
function InitUpDown(index) {
    var up = ".glyphicon-arrow-up";
    if (typeof (index) != "undefined") {
        up = ".j_slideads:gt(" + index + ") .glyphicon-arrow-up";
    }
    $(up).click(function () {
        var current = $(this).parents("li");
        var prev = current.prev("li");//获取当前节点的上一个元素
        if (typeof (prev.prop("outerHTML")) != "undefined") {
            togglePostion(current, prev);
        } else {
            $.dialog.errorTips('已到第一张');
            return;
        }
    });

    //下移
    var down = ".glyphicon-arrow-down";
    if (typeof (index) != "undefined") {
        down = ".j_slideads:gt(" + index + ") .glyphicon-arrow-down";
    }
    $(down).click(function () {
        var current = $(this).parents("li");
        var next = current.next("li");//获取当前节点的上一个元素
        if (typeof (next.prop("outerHTML")) != "undefined") {
            togglePostion(current, next);
        } else {
            $.dialog.errorTips('已到最后一张');
            return;
        }
    });

}
function togglePostion(a, b) {
    var a1 = $("<div id='a1'></div>").insertBefore(a);
    var b1 = $("<div id='b1'></div>").insertBefore(b);

    a.insertAfter(b1);
    b.insertAfter(a1);
    a1.remove();
    b1.remove();
    a1 = b1 = null;


}
function InitSelectDown() {
    $(".j_selectDownAd li").click(function () {
        var type = $(this).attr("type");
        var current = $(this).parent().parent().parent();
        if (type == 1) {
            var selectids = [];
            $.productSelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.id);
                });
                if (ids.length > 1) {
                    $.dialog.errorTips("只能选择一个商品");
                    return;;
                }
                current.find("input[type=text]").attr("value", '/product/detail/' + ids.join(","));
                current.find("input[type=text]").attr("disabled", true);
            }, "selleradmin", false);
        } else if (type == 2) {
            var selectids = [];
            $.couponSelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.game_id);
                });
                if (ids.length > 1) {
                    $.dialog.errorTips("只能选择一个优惠券");
                    return;;
                }
                current.find("input[type=text]").attr("disabled", true);
                current.find("input[type=text]").attr("value", '/product/detail/' + ids.join(","));
            }, null, false);
        }
        else if (type == 3) {
            var selectids = [];
            $.limitTimeBuySelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.Id);
                });
                if (ids.length > 1) {
                    $.dialog.errorTips("只能选择一个商品");
                    return;;
                }
                current.find("input[type=text]").attr("value", '/limittimebuy/detail/' + ids.join(","));
                current.find("input[type=text]").attr("disabled", true);
            }, null, false);
        } else {
            current.find("input[type=text]").val("/").removeAttr('disabled');
        }
    });
}