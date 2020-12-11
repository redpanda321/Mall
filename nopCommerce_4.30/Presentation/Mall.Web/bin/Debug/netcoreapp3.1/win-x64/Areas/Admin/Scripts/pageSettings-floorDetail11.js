$(function () {


    initNiceScroll();

    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    initImageUploader();
    bindSubmitClickEvent();
    bindHideSelector();
    initBrand();

    $('.floor-ex-img .ex-btn').hover(function () {
        $(this).parent().toggleClass('active');
    });
});



function initBrand() {
    /* 拼音查找逻辑代码 16进制生成字典查找 比数组循环快
     * 代码托管地址: https://github.com/githubyang/code/blob/master/charCode.js
     * 单骑闯天下 2014.12.8
     */
    var itemId = $('#homeFloorId').val();
    $.ajax({
        url: './GetBrandsAjax',
        data: 'id=' + itemId,
        type: 'post',
        success: function (d) {
            var data = d.data;
            ; (function (win, data, $, charCode) {
                var charList = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '+'],
                    getPinYin = function (str) {
                        if (typeof str !== 'string') {
                            return;
                        }
                        if ((/[0-9]+/).test(str[0])) {
                            return false;
                        }
                        if (contains(charList, ((str[0]).toUpperCase()))) {
                            return (str[0]).toUpperCase();
                        }
                        var index = str[0],
                            code = index.charCodeAt(0).toString(16);
                        if (char[code] !=null && char[code][0]) {
                            return char[code][0];
                        }
                    },
                    contains = function (arr, val) {
                        for (var i = 0, len = arr.length; i < len; i++) {
                            if (arr[i] == val) {
                                return 1;
                            }
                        }
                    },
                    create = function (e, bool) {
                        var str = (typeof e !== 'string') ? ($(e.target).text()) : e,
                            data = (str == '+' ? result['other'] : result[str]),
                            is = true,
                            string = '<div data-tag="' + str + '">';
                        if (!data) { return; }
                        $('.checkbox-group').children().each(function (i, e) {
                            var a = $(e).attr('data-tag');
                            if (a == str) {
                                is = false;
                                $(e).show();
                            } else {
                                $(e).hide();
                            }
                        });
                        if (!is && !bool) {
                            return;
                        }
                        for (var i = 0, len = data.length; i < len; i++) {
                            string += '<label class="checkbox-inline"><input class="brandCheckbox" type="checkbox" value="' + data[i].id + '" ' + (data[i].checked ? 'checked="true"' : '') + '>' + data[i].value + '</label>';
                        }
                        string += '</div>';
                        $('.checkbox-group').append(string);

                    },
                    result = {}, temp, html = '', uid = 1;
                for (var i = 0, len = data.length; i < len; i++) {
                    temp = getPinYin(data[i].value);
                    if (!temp) {
                        if (!result['other']) {
                            result['other'] = [];
                        }
                        result['other'].push({ id: data[i].id, checked: data[i].isChecked, value: data[i].value });
                    } else {
                        if (!result[temp]) {
                            result[temp] = [];
                        }
                        result[temp].push({ id: data[i].id, checked: data[i].isChecked, value: data[i].value });
                    }
                }
                for (var k = 0, l = charList.length; k < l; k++) {
                    charList[k] = (charList[k] == '+' ? 'other' : charList[k]);// 对其它非字母或者中文字符转换
                    if (result[charList[k]]) {
                        if (uid > 0) {
                            html += '<span class="btn-a ac"><a href="javascript:;" class="active">' + charList[k] + '</a></span>';
                            create(charList[k], 1);
                        } else {
                            html += '<span class="btn-a"><a href="javascript:;">' + (charList[k] == 'other' ? '+' : charList[k]) + '</a></span>';
                        }
                        // 选中生成
                        $.each(result[charList[k]], function (i, e) {
                            if (e.checked) {
                                $('#id_s').append('<label class="checkbox-inline"><input type="checkbox" checked="true" value="' + e.id + '" class="brandCheckbox">' + e.value + '</label>');
                            }
                        });
                        uid--;
                    } else {
                        html += '<span class="btn-a"><a href="javascript:;" style="color:#e4e4e4" class="disabled">' + (charList[k] == 'other' ? '+' : charList[k]) + '</a></span>';
                    }
                }
                $('#id_tab').html(html);
                $('#id_tab').bind('click', function (e) {// 点击开始按字母查找
                    var t = (e.target.nodeName.toUpperCase());
                    if (t != 'A') {
                        return;
                    }
                    if ($(e.target).hasClass('disabled')) {
                        return;
                    }
                    $(e.target).parent().siblings().filter('.ac').removeClass('ac').find('a').removeClass('active');
                    $(e.target).addClass('active').parent().addClass('ac');
                    create(e);
                });
                $('.checkbox-group').bind('click', function (e) {// 
                    var t = (e.target.nodeName.toUpperCase());
                    if (t != 'INPUT') {
                        return;
                    }
                    var str = $(e.target).parent().text(),
                        value = $(e.target).val();
                    if ($(e.target).prop('checked')) {
                        $('#id_s').append('<label class="checkbox-inline" style="margin-right:10px;"><input type="checkbox" checked="true" value="' + $(e.target).val() + '" class="brandCheckbox">' + str + '</label>');
                    } else {
                        $('#id_s').children().filter(function (i, e) {
                            var a = $(e).children().val();
                            if (value == a) {
                                return true;
                            }
                        }).remove();
                    }
                });
                $('#id_s').bind('click', function (e) {
                    var t = (e.target.nodeName.toUpperCase());
                    if (t != 'INPUT') {
                        return;
                    }
                    var value = $(e.target).val();
                    if (!$(e.target).prop('checked')) {
                        $(e.target).parent().remove();
                        $('.brandCheckbox').filter(function (i, e) {
                            var a = $(e).val();
                            if (value == a) {
                                return true;
                            }
                        }).attr('checked', false);
                    }
                });
            }(window, data, jQuery, char));
        }
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


function getSelectedBrands() {
    //获取选择品牌

    var brandsCheckBoxes = $('.choose-brand input[class="brandCheckbox"]');
    var brands = [];
    $.each(brandsCheckBoxes, function () {
        brands.push({ id: $(this).val() });
    });
    //if (brands.length > 8) {
    //    throw Error('品牌不能超过8个');
    //}
    console.log(brands);
    return brands;
}

function initImageUploader(imageUrl) {
    //初始化图片上传

    $("#one").MallUpload({
        title: '',
        imageDescript: '290*370',
        displayImgSrc: $("#one").attr('url'),
        imgFieldName: ""
    });
    $("#two").MallUpload({
        title: '',
        imageDescript: '398*199',
        displayImgSrc: $("#two").attr('url'),
        imgFieldName: ""
    });
    $("#three").MallUpload({
        title: '',
        imageDescript: '290*370',
        displayImgSrc: $("#three").attr('url'),
        imgFieldName: ""
    });
    $("#four").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#four").attr('url'),
        imgFieldName: ""
    });
    $("#five").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#five").attr('url'),
        imgFieldName: ""
    });
    $("#six").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#six").attr('url'),
        imgFieldName: ""
    });
    $("#seven").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#seven").attr('url'),
        imgFieldName: ""
    });
    $("#eight").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#eight").attr('url'),
        imgFieldName: ""
    });
    $("#nine").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#nine").attr('url'),
        imgFieldName: ""
    });
    $("#ten").MallUpload({
        title: '',
        imageDescript: '199*199',
        displayImgSrc: $("#ten").attr('url'),
        imgFieldName: ""
    });
}
function getPLink() {
    var plinks = $(".pLink");
    var pLinksArray = [];
    var index = 1;
    $.each(plinks, function () {
        var position = $(this).attr("position");
        var imageURL = $(this).parent().prev().find(".hiddenImgSrc").val();
        var url = $(this).val();
        if (!imageURL)
            throw Error('第' + index + '个区域未上传图片');
        if (!url)
            throw Error('第' + index + '个区域未填写链接');
        if (url.toLowerCase().indexOf('http://') < 0 && url.toLowerCase().indexOf('https://') < 0 && url.charAt(0) != '/') {
            throw Error('第' + index + '个链接地址请以"http://"或"https://"开头');
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
        brands: [],
        textLinks: [],
    };

    $('button[name="submit"]').click(function () {
        floorDetail.id = $('#homeFloorId').val();
        try {
            floorDetail.brands = getSelectedBrands();
            floorDetail.productLinks = getPLink();
            floorDetail.name = ''; 
            floorDetail.styleLevel = 3;
            submit(floorDetail);
        }
        catch (e) {
            _topPass(0);
            $.dialog.errorTips(e.message);
        }
    });
}