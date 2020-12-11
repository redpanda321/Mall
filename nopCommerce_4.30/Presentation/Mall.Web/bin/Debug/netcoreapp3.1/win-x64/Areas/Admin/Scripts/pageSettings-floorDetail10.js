$(function () {
    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    bindSubmitClickEvent();
    bindHideSelector();
    delProduct();

    $('.floor-ex-img .ex-btn').hover(function () {
        $(this).parent().toggleClass('active');
    });
    $('#addProducts').click(function () {

        var pids = $(".fid");
        var selectids = [];
        $.each(pids, function () {
            var ids = $(this).val();
            selectids.push(parseInt(ids));
        });

        $.productSelector.show(selectids, function (selectedProducts) {
            var ids = [];
            var str = "";
            var index = 0;
            $.each(selectedProducts, function () {
                index++;
                str += "<tr>";
                str += "<td><img src=\"" + this.imgUrl + "\" alt=\"\" />" + this.name + "</td>";
                str += "<td>" + this.price + "</td>";
                str += "<td align=\"center\">";
                str += "<input type=\"hidden\" value=\"" + this.id + "\" class=\"fid\" />";
                str += "<a href=\"javascript:;\" class=\"proremove\">删除</a></td>";
                str += "</tr>";
            });

            if (index > 30) {
                $.dialog.errorTips("商品不允许超过30个");
                return false;
            }
            else {
                $(".prol").html(str);
            }
            delProduct();
        });
    })
});

function delProduct() {
    //删除商品
    $(".proremove").click(function () {
        $(this).parent().parent().remove();
    })
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
//商品
function getProduct() {
    var pids = $(".fid");
    var pIdArray = [];
    var index = 0;
    $.each(pids, function () {
        var ids = $(this).val();
        pIdArray.push({ ProductId: ids });
        index++;
    });
    if (index == 0) {
        throw Error('请选择推荐商品!');
    } else if (index > 30) {
        throw Error('请选择商品不能超过30个!');
    }
    return pIdArray;
}




function bindSubmitClickEvent() {
    var floorDetail = {
        id: null,
        textLinks: [],
    };

    $('button[name="submit"]').click(function () {
        floorDetail.id = $('#homeFloorId').val();
        try {
            floorDetail.ProductModules = getProduct();
            floorDetail.name = getFloorName();
            floorDetail.styleLevel = 10;
            floorDetail.CommodityStyle = $("#CommodityStyle:checked").val();
            floorDetail.DisplayMode = $("#DisplayMode:checked").val();
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
