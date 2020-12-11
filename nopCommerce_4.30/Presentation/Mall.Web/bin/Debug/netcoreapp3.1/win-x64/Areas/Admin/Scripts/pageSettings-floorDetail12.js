$(function () {
    initNiceScroll();
    var imageUrl = $('input[name="imageCategoryImageUrl"]').val();
    initImageUploader();
    bindSubmitClickEvent();
    bindHideSelector();
	$('.floor-ex-img .ex-btn').hover(function(){
		$(this).parent().toggleClass('active');
	});
});
 
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
    if (brands.length > 8) {
        throw Error('品牌不能超过8个');
    }
    console.log(brands);
    return brands;
}

function initImageUploader(imageUrl) {
    //初始化图片上传

    $("#up29").MallUpload({
        title: '',
        imageDescript: '390*220',
        displayImgSrc: $("#up29").attr('url'),
        imgFieldName: ""
    } );

    $("#up30").MallUpload({
        title: '',
        imageDescript: '310*220',
        displayImgSrc: $("#up30").attr('url'),
        imgFieldName: ""
    });
    $("#up31").MallUpload({
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $("#up31").attr('url'),
        imgFieldName: ""
    });
    $("#up32").MallUpload({
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $("#up32").attr('url'),
        imgFieldName: ""
    });
    $("#up21").MallUpload({
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $("#up21").attr('url'),
        imgFieldName: ""
    });
    $("#up22").MallUpload({
        title: '',
        imageDescript: '230*220',
        displayImgSrc: $("#up22").attr('url'),
        imgFieldName: ""
    });
    $("#up23").MallUpload({
        title: '',
        imageDescript: '310*220',
        displayImgSrc: $("#up23").attr('url'),
        imgFieldName: ""
    });
    $("#up24").MallUpload({
        title: '',
        imageDescript: '390*220',
        displayImgSrc: $("#up24").attr('url'),
        imgFieldName: ""
    });
}
function GetStringLength( str )
{
    ///获得字符串实际长度，中文2，英文1 
    ///要获得长度的字符串 
    var realLength = 0, len = str.length, charCode = -1;
    for ( var i = 0; i < len; i++ )
    {
        charCode = str.charCodeAt( i );
        if ( charCode >= 0 && charCode <= 128 )
            realLength += 1;
        else realLength += 2;
    }
    return realLength;
};

function getPLink() {
    var plinks = $(".pLink");
    var pLinksArray = [];
    var index = 1;
    $.each(plinks, function () {
        var position = $(this).attr("position");
        var imageURL = $(this).parent().prev().find(".hiddenImgSrc").val();
        var url = $(this).val();
        if ( !imageURL && position != 29 && position != 30 && position != 31 && position != 32 )
        {
            throw Error( '第' + index + '个区域未上传图片' );
        }
        if ( !url && position != 29 && position != 30 && position != 31 && position != 32 )
        {
            throw Error( '第' + index + '个区域未填写链接' );
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
        floorDetail.id = $( '#homeFloorId' ).val();
        try {
            floorDetail.name = '';
            floorDetail.productLinks = getPLink();
            floorDetail.styleLevel = 12;
            submit(floorDetail);
        }
        catch (e) {
            _topPass(0);
            $.dialog.errorTips(e.message);
        }
    });
}