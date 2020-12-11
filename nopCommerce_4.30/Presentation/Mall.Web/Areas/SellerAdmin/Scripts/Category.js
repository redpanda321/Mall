function updateOrderOrName(actionName, param) {
    var loading = showLoading();
    ajaxRequest({
        type: 'POST',
        url: "/selleradmin/category/" + actionName,
        param: param,
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success == true) {
                $.dialog.tips('更新分类的' + (actionName == 'UpdateOrder' ? '显示顺序' : (actionName == 'UpdateCategoryShow' ? '显示' : '名称')) + '成功.', function () { location.reload(); });
            }
            else {
                $.dialog.errorTips(data.msg, function () { location.reload(); });
              
            }
        }
    });
}

function categoryTextEventBind() {
    var _order = 0;
    var _name = '';

    $('.container').on('focus', '.text-order', function () {
        _order = parseInt($(this).val());
    });
    $('.container').on('focus', '.text-name', function () {
        _name = parseInt($(this).val());
    });

    $('.container').on('blur', '.text-name,.text-order', function () {
        //var id = $(this).parent('td').find('.hidden_id').val();
        var id = $(this).attr("categoryId");
        if ($(this).hasClass('text-order')) {
            if (isNaN($(this).val()) || parseInt($(this).val()) <= 0) {
                $.dialog.errorTips("您输入的序号不合法,此项只能是大于零的整数！");
                $(this).val(_order);
            } else {
                if (parseInt($(this).val()) === _order) return;
                updateOrderOrName("UpdateOrder", { id: id, order: parseInt($(this).val()) });
            }
        } else {
            if ($(this).val().length === 0) {
                $.dialog.errorTips("分类名称不能为空！");
                $(this).val(_name);
            }
            else
                updateOrderOrName("UpdateName", { id: id, name: $(this).val() });
        }
    });
    $(".container").on('click', '.j_isshow', function () {
        var id = $(this).attr("categoryId");
        var isShow = $(this).attr("isshow") == 1 ? true : false;
        updateOrderOrName("UpdateCategoryShow", { id: id, isShow: isShow }, '显示');
    });
}

function initialdeleteCategory() {
    $('.container').on('click', '.delete-classify', function () {
        var msg = "";
        if ($(this).parents('tr').attr('class') == "level-2") {
            msg = "您确定要删除吗？";
        }
        else {
            msg = "删除该分类将会同时删除该分类的所有下级分类，您确定要删除吗？";
        }
        var id = $(this).attr("categoryId");
        $.dialog.confirm(msg, function () {
            ajaxRequest({
                type: 'POST',
                url: "/selleradmin/category/DeleteCategoryById",
                param: { id: id },
                dataType: "json",
                success: function (data) {
                    if (data.success == true) {
                        location.reload();
                    } else {
                        $.dialog.tips(data.msg);
                    }
                }, error: function () { }
            });
        }, function () {
            $.dialog.tips('你取消了操作');
        });
    });
}

function initialBatchDelete() {
    $("#deleteBatch").click(function () {
        var ids = [];

        $('table.category_table tbody tr.level-1').each(function () {
            var curRow = $(this);
            if (curRow.find('input[type=checkbox]').attr('checked') == 'checked') {
                ids.push(curRow.find('input.hidden_id').val());
            }
        });
        if (ids.length == 0) { $.dialog.tips('不能批量删除,因为您没有选中任何行.'); return; }
        $.dialog.confirm('确定删除选中行吗？', function () {
            var loading = showLoading();
            ajaxRequest({
                type: "POST",
                url: '/selleradmin/category/BatchDeleteCategory',
                param: { Ids: ids.join('|') },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success) {
                        location.reload();
                    } else {
                        $.dialog.errorTips("删除失败,请重试！", _this);
                    }
                },
                error: function (e) {
                    loading.close();
                    $.dialog.errorTips("删除失败,请重试！", _this);
                }
            });
        });
    });
}


function InitialDialog(option) {
    $.dialog({
        title: option.title,
        lock: true,
        id: 'addAtrr',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl" for="">分类名称</label><input value="' + option.name + '" id="newCategoryName" class="form-control input-sm" type="text" ><p id="nameErrorMsg" class="help-block">不能为空且不能多于12个字</p>',

            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl" for="">上级分类</label>',
                '<select class="form-control input-sm" id="categoryDrop"></select>',
            '</div>',
        '</div>'].join(''),
        init: function () {
            $("#newCategoryName").focus();
            $.ajax({
                type: 'GET',
                url: '/selleradmin/category/GetCategoryDrop',
                cache: false,
                data: { id: option.id },
                dataType: "json",
                success: function (data) {
                    if (data.success == true) {
                        var drop = $("#categoryDrop");
                        var html = [], cate = data.category;
                        for (var i = 0; i < cate.length; i++) {
                            var selected = cate[i].Selected == true ? "selected" : "";
                            html.push('<option ' + selected + ' value="' + cate[i].Value + '">' + cate[i].Text + '</option>');
                        }
                        $(drop).append($(html.join('')));
                    }
                    
                }
            });
        },
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var len = $("#newCategoryName").val().length;
            if (len > 12 || len <= 0) {
                $("#nameErrorMsg").css('color', 'red');
                $("#newCategoryName").focus();
                return false;
            }
            var name = $("#newCategoryName").val();
            var pId = parseInt($("#categoryDrop option:selected").val());

            var loading = showLoading();
            $.ajax({
                type: 'POST',
                url: '/selleradmin/category/CreateCategory',
                cache: false,
                data: { name: name, pId: pId },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        if (window.location.href.toLowerCase().indexOf('tar=sellercategory') > 0) {
                            if (window.opener && window.opener.BindSellerCategory) {
                                window.opener.BindSellerCategory();
                            }
                            window.close();
                        }
                        location.reload();
                    }
                    else {
                        $.dialog.errorTips(data.msg);
                    }
                }
            });

        }
    });
}
function saveState() {
    var hides = [];
    $('tr[cid]').each(function () {
        if ($(this).is(':hidden')) {
            hides.push($(this).attr('cid'));
        }
    });

    if (hides.length > 0) {
        $.cookie('CategoryHideItems', hides.join(','));
    }
}
$(function () {
    //新增分类
    categoryTextEventBind();

    initialdeleteCategory();
    initialBatchDelete();
	$('#btnlevel1').click(function () {
        $('table.category_table tbody span.glyphicon-minus-sign').removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
        $('table.category_table tbody tr.level-2,table.category_table tbody tr.level-3').hide();
        saveState();
    });
    $('#btnlevelAll').click(function () {
        $('table.category_table tbody span.glyphicon-plus-sign').removeClass('glyphicon-plus-sign').addClass('glyphicon-minus-sign');
        $('table.category_table tbody tr.level-2,table.category_table tbody tr.level-3').show();
        $.removeCookie('CategoryHideItems');
    });
    $('.check-all').click(function() {
    	var checkbox=$('.table').find('input[type=checkbox]');
    	if(this.checked){
    		checkbox.each(function(){this.checked = true})
    	}else{
    		checkbox.each(function(){this.checked = false})
    	}
    });


    $('.container').on('click', '.addCategory', function () {
        var id = $(this).attr('value');
        InitialDialog({ title: '添加分类', name: '', id: id })
    });
    if (window.location.href.toLowerCase().indexOf('tar=sellercategory') > 0) {
        $(".addCategory").click();
    }

    $('.level-1 .glyphicon').click(function () {
        var p = $(this).parents('.level-1');
        if ($(this).hasClass('glyphicon-plus-sign')) {
            var category = $(this).next('input').val();
            var url = "/selleradmin/category/GetCategoryByParentId";
            ajaxRequestForCategoryTree(this, category, url, 1);
        } else {
            $(this).removeClass('glyphicon-minus-sign').addClass('glyphicon-plus-sign');
            p.nextUntil('.level-1').remove();
        }
    });

    function ajaxRequestForCategoryTree(target, category, url, layer) {
        var loading = showLoading();
        $.ajax({
            type: 'GET',
            url: url,
            cache: false,
            data: { id: category },
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.success === true) {
                    var p = $(target).parents('.level-' + layer);
                    if (data.Category.length === 0) { $.dialog.tips('该分类下目前还没有子分类.'); return; }
                    var sub=[];
                    for (var i = 0; i < data.Category.length; i++) {
                        $(target).addClass('glyphicon-minus-sign').removeClass('glyphicon-plus-sign');
                        var left =  50;
                        var pix =  '├─────';
                        sub.push('<tr class="level-' + (layer + 1) + '">');
                            //'<td></td>',
                        sub.push('<td><s class="line" style="margin-left:' + left + 'px">' + pix + '</s>');
                        if (data.Category[i].Depth !== 2) {
                            sub.push('<span class="glyphicon glyphicon-plus-sign"></span>');
                        }
                        sub.push('<input class="hidden_id" type="hidden" value="' + data.Category[i].Id + '">');
                        sub.push('<input class="text-name" type="text" value="' + data.Category[i].Name + '"   categoryId="' + data.Category[i].Id + '" />');
                        sub.push('</td>');
                        sub.push('<td class="Sclear-C5">');
                        sub.push('<input class="text-order" type="text" value="' + data.Category[i].DisplaySequence + '" categoryId="' + data.Category[i].Id + '"  />');
                        sub.push('</td>');
                        sub.push('<td style="text-align:center">');
                        sub.push('<span class="btn-a">');
                        if (data.Category[i].IsShow) {
                            sub.push('<a categoryId="' + data.Category[i].Id + '" IsShow="1" class="j_isshow">√</a>');
                        } else {
                            sub.push('<a categoryId="' + data.Category[i].Id + '" IsShow="0" class="j_isshow">×</a>');
                        }
                        sub.push('</span>');
                        sub.push('</td>');
                        //sub.push('<input class="text-order" type="text" value="' + data.Category[i].DisplaySequence + '"/></td>');
                        sub.push('<td class="td-operate">');
                        sub.push('<span class="btn-a">');

                        sub.push('<a class="delete-classify" categoryId="' + data.Category[i].Id + '" >删除</a></span>');
                        sub.push('</td>');
                        sub.push('</tr>');
                       
                    }
                    p.after(sub.join(''));
                    
                }

            },
            error: function () {
                loading.close();

            }
        });
    };
});





