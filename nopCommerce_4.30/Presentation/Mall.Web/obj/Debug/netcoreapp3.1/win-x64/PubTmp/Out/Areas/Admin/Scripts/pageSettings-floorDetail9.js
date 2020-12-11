$(function () {

    initNiceScroll();

    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    initImageUploader();
    bindTextLinkBtnClickEvent();
    bindProductsBtnClickEvent();
    bindSubmitClickEvent();
    bindHideSelector();

    $('.floor-ex-img .ex-btn').hover(function () {
        $(this).parent().toggleClass('active');
    });
});

function bindTextLinkBtnClickEvent() {
    var html = ['<tr type="textLink" name="">',
                     , '<td><input class="form-control input-xs" type="text" name="name" value="" /></td>'
                     , '<td><input class="form-control input-xs" type="text" name="url" value="/" /><div class="downlist"><span>请选择</span><ul><li onclick="setTextlink(this)" url="1">选择商品</li><li onclick="setTextlink(this)" url="/IntegralMall">积分商城</li><li onclick="setTextlink(this)" url="3">PC专题</li><li onclick="setTextlink(this)" url="2">自定义链接</li><li onclick="setTextlink(this)" url="/LimitTimeBuy/home">限时抢购页</li><li onclick="setTextlink(this)" url="4">商品分类</li></ul></div></td>'
                     , '<td class="td-operate"><span class="btn-a"><a>删除</a></span></td>'
             , '</tr>'].join();

    //添加textLink表单
    $('#addTextLink').click(function () {
        if ($('tr[type="textLink"]').length === 8) { $.dialog.tips('文本链接最多添加8个.'); return; }
        $('tbody[type="textLink"]').append(html);
    });

    //textLink的删除操作
    $('tbody[type="textLink"]').on('click', 'a', function () {
        $(this).parents('tr').remove();
    });
}

function bindHideSelector() {

    $('.choose-checkbox').each(function () {
        var _this = $(this);
        $(this).find('a').click(function () {
            $(this).parent().hide().siblings().fadeIn();
            var h = _this.find('.scroll-box').height();
            _this.find('.enter-choose').show();
            _this.animate({ height: h }, 200);
            _this.css({ paddingBottom: '50px' });
        })

        $(this).find('.btn').click(function () {
            _this.height(_this.find('.scroll-box').height());
            _this.css({ padding: 0 }).find('.scroll-box').hide().siblings('.choose-selected').fadeIn();
            _this.find('.enter-choose').hide();
            _this.animate({ height: '43px' }, 200);
            var id = $(this).attr('name');
            getSelectedText(id);
        });
    });
}



function getSelectedText(type) {
    var text = [];
    if (type == 'categoryBtn') {
        var categoryCheckBoxes = $('input[name="category"]:checked');
        $.each(categoryCheckBoxes, function () {
            text.push($(this).parent().text());
        });
        $('#selectedCategories').html(text.join(','));
    }
    else {
        var brandsCheckBoxes = $('input[name="brand"]:checked');
        $.each(brandsCheckBoxes, function () {
            text.push($(this).parent().text());
        });
        $('#selectedBrands').html(text.join(','));
    }

}


function initNiceScroll() {
    //初始化NiceScroll
    if (+[1, ]) {
        $(".scroll-box").niceScroll({
            styler: 'fb',
            cursorcolor: "#7B7C7E",
            cursorwidth: 6,
        });
    }
}


function initImageUploader(imageUrl) {
    //初始化图片上传

    $("#up0").MallUpload({
        title: '',
        imageDescript: '200*100',
        displayImgSrc: $("#up0").attr('url'),
        imgFieldName: ""
    });

    $("#up1").MallUpload({
        title: '',
        imageDescript: '200*260',
        displayImgSrc: $("#up1").attr('url'),
        imgFieldName: ""
    });
    $("#up2").MallUpload({
        title: '',
        imageDescript: '400*360',
        displayImgSrc: $("#up2").attr('url'),
        imgFieldName: ""
    });
    $("#up3").MallUpload({
        title: '',
        imageDescript: '400*180',
        displayImgSrc: $("#up3").attr('url'),
        imgFieldName: ""
    });
    $("#up4").MallUpload({
        title: '',
        imageDescript: '400*180',
        displayImgSrc: $("#up4").attr('url'),
        imgFieldName: ""
    });
    $("#up5").MallUpload({
        title: '',
        imageDescript: '190*120',
        displayImgSrc: $("#up5").attr('url'),
        imgFieldName: ""
    });
    $("#up6").MallUpload({
        title: '',
        imageDescript: '190*120',
        displayImgSrc: $("#up6").attr('url'),
        imgFieldName: ""
    });
    $("#up7").MallUpload({
        title: '',
        imageDescript: '190*120',
        displayImgSrc: $("#up7").attr('url'),
        imgFieldName: ""
    });
    $("#up8").MallUpload({
        title: '',
        imageDescript: '238*180',
        displayImgSrc: $("#up8").attr('url'),
        imgFieldName: ""
    });
    $("#up9").MallUpload({
        title: '',
        imageDescript: '238*180',
        displayImgSrc: $("#up9").attr('url'),
        imgFieldName: ""
    });
    $("#up10").MallUpload({
        title: '',
        imageDescript: '238*180',
        displayImgSrc: $("#up10").attr('url'),
        imgFieldName: ""
    });
    $("#up11").MallUpload({
        title: '',
        imageDescript: '238*180',
        displayImgSrc: $("#up11").attr('url'),
        imgFieldName: ""
    });
    $("#up12").MallUpload({
        title: '',
        imageDescript: '238*180',
        displayImgSrc: $("#up12").attr('url'),
        imgFieldName: ""
    });
}


function bindProductsBtnClickEvent() {
    var html = ['<tr type="Products" name="">',
                     , '<td><input class="form-control input-xs" type="text" name="name" value="" /></td>'
                     , '<td><span ids="">0</span></td>'
                     , '<td class="td-operate"><span class="btn-a"><a>选择商品</a></span><span class="btn-del"><a href="#">删除</a></span></td>'
             , '</tr>'].join();

    $('#addProducts').click(function () {
        if ($('tr[type="Products"]').length === 4) { $.dialog.tips('选项卡最多添加4个.'); return; }
        $('tbody[type="Products"]').append(html);
        $('tbody[type="Products"] span[class="btn-a"] a').unbind("click");
        $('tbody[type="Products"] span[class="btn-del"] a').unbind("click");
        $('tbody[type="Products"] span[class="btn-a"] a').click(function () {
            var aSelf = this;
            var ids = $(aSelf).parents("tr").find("td span")[0].getAttribute("ids");
            var selectids = [];

            if (ids != null && ids != undefined && ids.length > 0) {
                var array = ids.split(',');
                for (var i = 0 ; i < array.length; i++) {
                    selectids.push(parseInt(array[i]));
                }
            }
            $.productSelector.show(selectids, function (selectedProducts) {
                var ids = [];
                $.each(selectedProducts, function () {
                    ids.push(this.id);
                });

                if (ids.length > 10) {
                    $.dialog.errorTips("商品不允许超过10个");
                    return false;
                }
                else {
                    $(aSelf).parents("tr").find("td span")[0].innerHTML = ids.length;
                }

                $(aSelf).parents("tr").find("td span")[0].setAttribute("ids", ids);
            });
        })
        $('tbody[type="Products"] span[class="btn-del"] a').click(function () {
            $(this).parent().parent().parent().remove();
        })
    });


    $('tbody[type="Products"] span[class="btn-a"] a').click(function () {
        var aSelf = this;
        var ids = $(aSelf).parents("tr").find("td span")[0].getAttribute("ids");
        var selectids = [];

        if (ids != null && ids != undefined && ids.length > 0) {
            var array = ids.split(',');
            for (var i = 0 ; i < array.length; i++) {
                selectids.push(parseInt(array[i]));
            }
        }
        $.productSelector.show(selectids, function (selectedProducts) {
            var ids = [];
            $.each(selectedProducts, function () {
                ids.push(this.id);
            });

            if (ids.length > 10) {
                $.dialog.errorTips("商品不允许超过10个");
                return false;
            }
            else {
                $(aSelf).parents("tr").find("td span")[0].innerHTML = ids.length;
            }
            $(aSelf).parents("tr").find("td span")[0].setAttribute("ids", ids);
        });
    })

    $('tbody[type="Products"] span[class="btn-del"] a').click(function () {
        $(this).parent().parent().parent().remove();
    })

}




function GetStringLength(str) {
    ///获得字符串实际长度，中文2，英文1 
    ///要获得长度的字符串 
    var realLength = 0, len = str.length, charCode = -1;
    for (var i = 0; i < len; i++) {
        charCode = str.charCodeAt(i);
        if (charCode >= 0 && charCode <= 128)
            realLength += 1;
        else realLength += 2;
    }
    return realLength;
};
function getTextLinks() {
    var textLinkRows = $('tr[type="textLink"]');
    var textLinks = [];
    $.each(textLinkRows, function () {
        var id = $(this).attr('name');
        var name = $(this).find('input[name="name"]').val();
        var url = $(this).find('input[name="url"]').val();
        if (!name || !url) {
            throw Error('请将文字链接填写完整');
        }
        else if (GetStringLength(name) > 12) {
            throw Error('文本模块名称不能超过6个字符');
        }
        textLinks.push({ id: id ? id : 0, name: name, url: url });
    });
    //if (textLinks.length === 0) {
    //    throw Error('未设置任何文本模块');
    //}
    return textLinks;
}
function getPLink() {
    var plinks = $(".pLink");
    var pLinksArray = [];
    var index = 1;
    $.each(plinks, function () {
        var position = $(this).attr("position");
        var imageURL = $(this).parent().prev().find(".hiddenImgSrc").val();
        var url = $(this).val();
        var text = $(this).parents('tr').find('td').eq(0).text();
        if (!imageURL) {
            throw Error(text + ' 区域未上传图片');
        }
        if (!url) {
            throw Error(text + ' 区域未填写链接');
        }
        if (url.toLowerCase().indexOf('http://') < 0 && url.toLowerCase().indexOf('https://') < 0 && url.charAt(0) != '/') {
            throw Error(text + ' 区域链接地址请以"http://"或"https://"开头');
        }
        pLinksArray.push({ id: position, name: imageURL, url: url });
        index++;
    });
    console.log(pLinksArray);
    return pLinksArray;
}





function bindSubmitClickEvent() {
    var floorDetail = {
        id: null,
        brands: []
    };

    $('button[name="submit"]').click(function () {
        floorDetail.id = $('#homeFloorId').val();
        floorDetail.DefaultTabName = $('#DefaultTabName').val();
        try {
            if (floorDetail.DefaultTabName.length < 1) {
                throw Error('请填写默认选项卡名称');
            } else if (floorDetail.DefaultTabName.length > 6) {
                throw Error('默认选项卡名称不能超过6个字符');
            }

            floorDetail.textLinks = getTextLinks();
            floorDetail.productLinks = getPLink();
            floorDetail.name = getFloorName();
            floorDetail.subName = $("#SubName").val();
            floorDetail.styleLevel = 8;
            submit(floorDetail);
        }
        catch (e) {
            _topPass(0);
            $.dialog.errorTips(e.message);
        }
    });
}

function getFloorName() {
    var name = $("#FloorName").val().replace(/(^\s*)|(\s*$)/g, '');
    if (name.replace(/[ ]/g, "").length == 0) {
        $("#FloorName").focus();
        throw Error('请填写楼层名称');
    }
    if (name.length <= 1) {
        $("#FloorName").focus();
        throw Error('楼层名称最少为两位字符');
    }
    if (name.length > 10) {
        $("#FloorName").focus();
        throw Error('楼层名称长度不得大于10个字符');
    }
    return name;
}

