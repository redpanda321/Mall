$(function () {
    $('#addExpress').click(function () {
        showAddWindow();
    });
    $('input[type="checkbox"]').onoff();
    $('input.expressCheck').change(function () {
        var _this = $(this),
            state = _this[0].checked ? 1 : 0,
            expressid = $(this).attr('expressid'),
            loading = showLoading();
        $.post('./ChangeStatus', { id: expressid, status: state }, function (result) {
            loading.close();
            if (!result.success) {
                $.dialog.errorTips('操作失败!失败原因：' + result.msg);
            }
        }, "json");
    });
}
);
function deleteExpress(id,name)
{
    $.dialog.confirm('确定删除快递公司:' + name + ' 吗？', function () {
        var loading = showLoading();
        $.get('deleteexpress', { id: id }, function (result) {
            if (loading) {
                loading.close();
            }
            if (result.success) {
                location.reload();
                $.dialog.tips('删除成功！');
            }
            else {
                $.dialog.tips('操作异常：' + result.msg);
            }
        });
    });
    
}
function showAddWindow(id,company, kuaidiniao, kuaidi100)
{
    id = id || 0;
    company = company || '';
    kuaidiniao = kuaidiniao || '';
    kuaidi100 = kuaidi100|| '';
    $.dialog({
        title: id == 0 ? '添加物流公司' :'编辑物流公司',
        width: 466,
        lock: true,
        id: 'goodCheck',
        content: [
            '<div class="form-group" style="position:relative">',
            '<label class="label-inline fl">公司名称</label>',
            '<p class="only-text"><input class="form-control" type="text" name="" id="txtCompany" value=\"' + company.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '\"/></p>',
            '</div>',
            '<div class="form-group" style="position:relative">',
            '<label class="label-inline fl">快递鸟code</label>',
            '<p class="only-text"><input class="form-control" type="text" name="" id="txtKuaidiniaoCode" value=\"' + kuaidiniao.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '\"/></p>',
            '</div>',
            '<div class="form-group" style="position:relative">',
            '<label class="label-inline fl">快递100code</label>',
            '<p class="only-text"><input class="form-control" type="text" name="" id="txtKuaidi100Code" value=\"' + kuaidi100.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '\"/></p>',
            '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txtCompany").focus(); },
        button: [
            {
                name: '确定',
                callback: function () {
                    company = $('#txtCompany').val();
                    kuaidi100 = $('#txtKuaidi100Code').val();
                    kuaidiniao = $('#txtKuaidiniaoCode').val();
                    if (company == '')
                    {
                        $.dialog.alert('快递公司名称，不能为空');
                        return false;
                    }
                    if (kuaidiniao == '' && kuaidi100 == '')
                    {
                        $.dialog.alert('快递公司Code，不能为空');
                        return false;
                    }
                    $.post('Express', { Id: id, Name: company, Kuaidi100Code: kuaidi100, KuaidiNiaoCode: kuaidiniao }, function (result) {
                        if (result.success)
                        {
                            location.reload();
                            $.dialog.tips('保存成功！');
                        }
                        else {
                            $.dialog.tips('操作异常：' + result.msg);
                        }
                    });               
                },
                focus: true
            },
            {
                name: '取消',
                callback: function () {
                                       
                },
                focus: false
            }]
    });
}

