// JavaScript source code
$(function () {
    init();
})

function init() {
    $("#btnSave").click(function () {
        var loading = showLoading();
        var str = "";
        var cityids = "";
        $(".show-region-real").each(function () {
            if ($(this).prop("checked")) {
                str += "'" + $(this)[0].id + "',";
            }
        })
        str = str.substr(0, str.length - 1);

        $(".show-region-ckb").each(function () {
            if ($(this).prop("checked")) {
                cityids += "'" + $(this)[0].id + "',";
            }
        })
        cityids = cityids.substr(0, cityids.length - 1);
        $.post("Save", { addressIds: str, addressIds_city: cityids }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips(result.msg);
            }
        })
    })

    $(".sc1").click(function () {
        $(".hidereginDiv").hide();
        $(this).parent().parent().find(" ul").hide();
        $(this).parent().parent().find(".hidereginDiv").show();
    })

    $(".show-region").click(function () {
        $(this).parent().parent().find(".hidereginDiv ul").hide();
        $(this).parent().find("ul").show();
    })

    $(".show-region-ckb").change(function () {
    	var _this=$(this),
    		parentEl=_this.parent(),
    		province=_this.parents('.dl-horizontal').find('h4 input');
        if (_this.prop("checked")) {
            parentEl.find("li input[type=checkbox]").attr("checked", 'true');
        }
        else {
            parentEl.find("li input[type=checkbox]").removeAttr("checked");
        }
        
        if(parentEl.parent().find('.show-region-ckb').length==parentEl.parent().find('.show-region-ckb:checked').length){
    		province[0].checked=true;
    	}else{
    		province[0].checked=false;
    	}
    })

    $(".show-region-real").change(function () {
    	var _this=$(this);
    	
    	_this.parents('.hidereginDiv').find('.show-region-ckb')[0].checked=(_this.parents('ul').find('input').length==_this.parents('ul').find('input:checked').length)
    	
        var province=_this.parents('.dl-horizontal').find('h4 input'),
        	city=_this.parents('.dl-horizontal').find('h5 .show-region-ckb'),
        	cityChecked=_this.parents('.dl-horizontal').find('h5 .show-region-ckb:checked');
        	
    	province[0].checked=(city.length==cityChecked.length);
    })

    $(".region-select span").click(function () {
        $(this).parent().hide();
    })
    
    $('.dl-horizontal').each(function(){
    	var _this=$(this);
    	if(_this.find('.show-region-ckb').length==_this.find('.show-region-ckb:checked').length){
    		_this.find('h4 input')[0].checked=true;
    	}
    });
    
    $('.check-all').change(function(){
    	var checked=this.checked;
    	$(this).parent().siblings().find('input').each(function(){
    		this.checked=checked;
    	})
    });
}