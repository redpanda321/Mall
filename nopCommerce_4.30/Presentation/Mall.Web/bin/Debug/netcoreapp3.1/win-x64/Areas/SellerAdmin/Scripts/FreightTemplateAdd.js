(function () {
    var templateId, ulAreaData, ulAreaData2, freightDetailData, shippingfreeData, liDataKey = 'listItem';

    $(function () {
        //需引用vue.js CDN地址:http://cdn.jsdelivr.net/vue/1.0.7/vue.min.js
        //vue.js 是属性发生改变就会更新文档，无需手动刷新
        ulAreaData = new Vue({
            el: '#ulArea',
            data: {
                leftProvince: getListData(0),
                editRowIndex: 0,//当前编辑区域的行序号
                rightProvice: []
            }, computed: {
                // a computed getter
                currentRightProvice: function () {
                    while (this.rightProvice.length <= this.editRowIndex)
                        this.rightProvice.push([]);
                    return this.rightProvice[this.editRowIndex];
                }
            }
        });
        ulAreaData2 = new Vue({
            el: '#ulArea2',
            data: {
                leftProvince: getListData(0),
                editRowIndex: 0,//当前编辑区域的行序号
                rightProvice: []
            }, computed: {
                // a computed getter
                currentRightProvice: function () {
                    while (this.rightProvice.length <= this.editRowIndex)
                        this.rightProvice.push([]);
                    return this.rightProvice[this.editRowIndex];
                }
            }
        });
        freightDetailData = new Vue({
            el: 'table.table-area-freight>tbody',
            data: {
                freightDetails: []
            },
            methods: {
                edit: function (index) {
                    var _this = this;
                    ulAreaData.editRowIndex = index;

                    //弹窗
                    $.dialog({
                        title: '选择区域',
                        lock: true,
                        width: 700,
                        id: 'logoArea',
                        content: $("#ulArea")[0],
                        padding: '20px 30px',
                        okVal: '保存',
                        ok: function () {
                            _this.freightDetails[index].showSelectData = showSelect(ulAreaData.currentRightProvice);
                        }
                    });
                },
                remove: function (index) {
                    ulAreaData.editRowIndex = index;
                    var list = ulAreaData.currentRightProvice;
                    while (list.length > 0) {
                        var data = list[0];
                        removeChildren(list, data);

                        var parent = createParent(ulAreaData.leftProvince, data);
                        addChildren(parent, data);
                    }

                    ulAreaData.rightProvice.removeAt(index);
                    this.freightDetails.removeAt(index);
                }
            },
            created: function () {
                $('table.table-area-freight>tbody').show();
            }
        });
        //指定地区包邮
        shippingfreeData = new Vue({
            el: 'table.table-shippingfree>tbody',
            data: {
                freightDetails: []
            },
            methods: {
                edit: function (index) {
                    var _this = this;
                    ulAreaData2.editRowIndex = index;

                    //弹窗
                    $.dialog({
                        title: '选择区域',
                        lock: true,
                        width: 700,
                        id: 'logoArea2',
                        content: $("#ulArea2")[0],
                        padding: '20px 30px',
                        okVal: '保存',
                        ok: function () {
                            _this.freightDetails[index].showSelectData = showSelect(ulAreaData2.currentRightProvice);
                        }
                    });
                },
                remove: function (index) {
                    ulAreaData2.editRowIndex = index;
                    var list = ulAreaData2.currentRightProvice;
                    while (list.length > 0) {
                        var data = list[0];
                        removeChildren(list, data);

                        var parent = createParent(ulAreaData2.leftProvince, data);
                        addChildren(parent, data);
                    }

                    ulAreaData2.rightProvice.removeAt(index);
                    this.freightDetails.removeAt(index);
                    if (this.freightDetails.length <= 0) {
                        $("#ckbSpecifications").attr("checked", false);
                        $("#regionFree").hide();
                    }
                }
            },
            created: function () {
                $('table.table-shippingfree>tbody').show();
            }
        });
        templateId = $('#Id').val();
        if ($.notNullOrEmpty(templateId) && templateId > 0) {
            $.get('GetFreightAreaDetail?templateId={0}'.format(templateId), function (result) {
                var data = result.model;
                if (data && data.length > 0) {
                    data = data.where(function (p) { return p.isDefault == 0; });

                    var selects = [];
                    for (var i = 0; i < data.length; i++) {
                        var item = data[i];

                        var list = [];

                        for (var j = 0; j < item.freightAreaDetail.length; j++) {
                            var area = item.freightAreaDetail[j];
                            var province = ulAreaData.leftProvince.first(function (p) { return p.Id == area.provinceId; });
                            if (area.cityId) {
                                getListData(area.provinceId, province);

                                var city = province.childrens.first(function (p) { return p.Id == area.cityId; });
                                if (area.countyId) {
                                    getListData(area.cityId, city);

                                    var county = city.childrens.first(function (p) { return p.Id == area.countyId; });
                                    if (area.townIds) {
                                        getListData(area.countyId, county);

                                        var townIds = area.townIds.split(',');
                                        for (var k = 0; k < townIds.length; k++) {
                                            var town = county.childrens.first(function (p) { return p.Id == townIds[k]; });
                                            list.push(town);
                                        }
                                    } else
                                        list.push(county);
                                } else
                                    list.push(city);
                            } else
                                list.push(province);
                        }

                        selects.push(list);
                    }

                    for (var i = 0; i < data.length; i++) {
                        ulAreaData.editRowIndex = i;
                        var list = selects[i];

                        for (var j = 0; j < list.length; j++) {
                            var listItem = list[j];
                            removeChildren(ulAreaData.leftProvince, listItem);
                            var parent = createParent(ulAreaData.currentRightProvice, listItem);
                            addChildren(parent, listItem);
                        }

                        var detail = $.extend(data[i], { showSelectData: showSelect(ulAreaData.currentRightProvice) });
                        freightDetailData.freightDetails.push(detail);
                    }
                }
                //指定地区包邮
                var data2 = result.modelfree;
                if (data2 && data2.length > 0) {
                    $("#ckbSpecifications").attr("checked", 'checked');
                    $("#regionFree").show();
                    var selects2 = [];
                    for (var i = 0; i < data2.length; i++) {
                        var item = data2[i];

                        var list = [];

                        for (var j = 0; j < item.freightAreaDetail.length; j++) {
                            var area = item.freightAreaDetail[j];
                            var province = ulAreaData2.leftProvince.first(function (p) { return p.Id == area.provinceId; });
                            if (area.cityId) {
                                getListData(area.provinceId, province);

                                var city = province.childrens.first(function (p) { return p.Id == area.cityId; });
                                if (area.countyId) {
                                    getListData(area.cityId, city);

                                    var county = city.childrens.first(function (p) { return p.Id == area.countyId; });
                                    if (area.townIds) {
                                        getListData(area.countyId, county);

                                        var townIds = area.townIds.split(',');
                                        for (var k = 0; k < townIds.length; k++) {
                                            var town = county.childrens.first(function (p) { return p.Id == townIds[k]; });
                                            list.push(town);
                                        }
                                    } else
                                        list.push(county);
                                } else
                                    list.push(city);
                            } else
                                list.push(province);
                        }

                        selects2.push(list);
                    }

                    for (var i = 0; i < data2.length; i++) {
                        ulAreaData2.editRowIndex = i;
                        var list = selects2[i];

                        for (var j = 0; j < list.length; j++) {
                            var listItem = list[j];
                            removeChildren(ulAreaData2.leftProvince, listItem);
                            var parent = createParent(ulAreaData2.currentRightProvice, listItem);
                            addChildren(parent, listItem);
                        }

                        var detail = $.extend(data2[i], { showSelectData: showSelect(ulAreaData2.currentRightProvice) });
                        shippingfreeData.freightDetails.push(detail);
                    }
                }
            });
        }

        //初始化页面
        $('#inputDefFirstUnit').val(initDefFirst);
        $('#inputDefFirstUnitMonry').val(initDefFirstMoney);
        $('#inputDefAccumulationUnit').val(initDefAccumulationUnit);
        $('#inputDefAccumulationUnitMoney').val(initDefAccumulationUnitMoney);

        $("#regionSelector").RegionSelector({
            selectClass: "form-control input-sm select-sort",
            valueHidden: "#SourceAddress"
        });

        $('#radioSelfDef,#radioSellerDef').click(function () {
            SetFreeStatus();
        });
        $('#radioPiece,#radioWeight,#radioBulk').click(function () {
            setUnit();
        });
        //根据计价方式设置单位
        setUnit();
        //根据是否包邮隐藏地区运费
        SetFreeStatus();

        $('#btnSave').click(function () {
            SaveData();
        });

        //新增行
        $('#addCityFreight').click(function () {
            freightDetailData.freightDetails.push({ showSelectData: [] });
        });
        //新增指定城市包邮行
        $('#addFree').click(function () {
            shippingfreeData.freightDetails.push({ showSelectData: [], conditionType: 1 });
            regionFreeChange();
        });

        $('a[name="delContent"]').click(function () {
            $(this).parent().parent().parent().remove();
        });

        //弹框显示市级
        $(document).on('click', '.operate', function () {
            var cityDiv = $(this).siblings('div');
            if (cityDiv.is(':hidden')) {
                $('.city-box').hide();
                cityDiv.show();
            } else {
                cityDiv.hide();
            }

        });
        if (IsUsed == 1) {
            $('input[name="valuationMethod"]').attr('disabled', 'disabled');
            $('#valuationMethodTip').text('已使用，不能修改');
        }
        //弹框关闭市级
        $(document).on('click', '.city .colse', function () {
            $(this).parents('.city-box').hide();
        });
        $('#ckbSpecifications').change(function () {
            $('#ckbSpecifications').toggleClass('hidden');
            if (this.checked == true) {
                $('#addFree').click();
                $("#regionFree").show();
            }
            else {
                $("#regionFree").hide();
                shippingfreeData.freightDetails = [];//清空数组集合
            }
        });
    });

    function getListData(id, parent) {
        if (parent && parent.initChildren == true)
            return;

        var listData = [];
        $.ajax({
            url: '/common/RegionAPI/GetSubRegion',
            type: 'POST', //GET
            async: false,    //或false,是否异步
            data: {
                parent: id
            },
            timeout: 5000,    //超时时间
            dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
            success: function (data) {
                sortList(data);
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    item.showChildren = false;
                    item.initChildren = false;
                    item.childrens = [];
                    item.childrensLength = 0;
                    item.isFull = function () {//判断是否包含所有下级
                        var fullChildrens = this.childrens.sum(function (p) { return p.isFull() ? 1 : 0; });//sum方法是申明在CommonJS.js里面的Array的扩展方法
                        return fullChildrens == this.childrensLength;
                    };
                    if (parent) {
                        item.parent = parent;
                        parent.childrens.push(item);//属性发生改变自动刷新文档
                        parent.childrensLength++;//此属性只能在getListData后赋值
                        parent.initChildren = true;
                    }
                }
                listData = data;
            }
        });

        return listData;
    }

    function openChildren(id) {
        var _this = $(this);
        var li = _this.parent();

        var item = findItemData(id, li);
        if (item.initChildren == false)
            getListData(id, item);
        item.showChildren = !item.showChildren;//属性发生改变自动刷新文档

        return false;
    }
    function openChildren2(id) {
        var _this = $(this);
        var li = _this.parent();

        var item = findItemData2(id, li);
        if (item.initChildren == false)
            getListData(id, item);
        item.showChildren = !item.showChildren;//属性发生改变自动刷新文档

        return false;
    }

    //找到文档节点对应的数据
    function findItemData(id, li) {
        var item = li.data(liDataKey);
        if (item == null) {
            var items;
            var parentLi = li.parent().closest('li');
            if (parentLi.length == 0)
                items = li.closest('ul.ul_first.unused').length > 0 ? ulAreaData.leftProvince : ulAreaData.currentRightProvice;
            else
                items = findItemData(parentLi.attr('id'), parentLi).childrens;

            for (var i = 0; i < items.length; i++) {
                var temp = items[i];
                if (temp.Id == id) {
                    li.data(liDataKey, temp);
                    return temp;
                }
            }
        }
        return item;
    }
    function findItemData2(id, li) {
        var item = li.data(liDataKey);
        if (item == null) {
            var items;
            var parentLi = li.parent().closest('li');
            if (parentLi.length == 0)
                items = li.closest('ul.ul_first.unused').length > 0 ? ulAreaData2.leftProvince : ulAreaData2.currentRightProvice;
            else
                items = findItemData2(parentLi.attr('id'), parentLi).childrens;

            for (var i = 0; i < items.length; i++) {
                var temp = items[i];
                if (temp.Id == id) {
                    li.data(liDataKey, temp);
                    return temp;
                }
            }
        }
        return item;
    }

    function selectItem(e, id) {
        e = e ? e : window.event;

        if (window.event) { // IE  
            e.cancelBubble = true;
        } else { // FF  
            //e.preventDefault();   
            e.stopPropagation();
        }

        $(this).parent().toggleClass("selected");
        return false;
    }

    //添加
    function addItem() {
        $('#ulArea ul.ul_first.unused li.selected').each(function () {
            var li = $(this);
            var data = findItemData(li.attr('id'), li);

            removeChildren(ulAreaData.leftProvince, data);
            var parent = createParent(ulAreaData.currentRightProvice, data);
            addChildren(parent, data);
        });
    }
    function addItem2() {
        $('#ulArea2 ul.ul_first.unused li.selected').each(function () {
            var li = $(this);
            var data = findItemData2(li.attr('id'), li);

            removeChildren(ulAreaData2.leftProvince, data);
            var parent = createParent(ulAreaData2.currentRightProvice, data);
            addChildren(parent, data);
        });
    }

    //移除
    function removeItem() {
        $('#ulArea ul.ul_first.used li.selected').each(function () {
            var li = $(this);
            var data = findItemData(li.attr('id'), li);

            removeChildren(ulAreaData.currentRightProvice, data);
            var parent = createParent(ulAreaData.leftProvince, data);
            addChildren(parent, data);
        });
    }
    function removeItem2() {
        $('#ulArea2 ul.ul_first.used li.selected').each(function () {
            var li = $(this);
            var data = findItemData2(li.attr('id'), li);

            removeChildren(ulAreaData2.currentRightProvice, data);
            var parent = createParent(ulAreaData2.leftProvince, data);
            addChildren(parent, data);
        });
    }

    function addChildren(parent, item) {
        for (var i = 0; i < item.childrens.length; i++) {
            var children = item.childrens[i];
            var exitChildrent = parent.childrens.first(function (p) { return p.Id == children.Id });//first方法是申明在CommonJS.js里面的Array的扩展方法
            if (exitChildrent) {
                addChildren(exitChildrent, children);
            }
            else {
                parent.childrens.push(children);
                children.parent = parent;
            }
        }
        sortList(parent.childrens);
    }

    function createParent(list, item) {
        var parent;
        if (item.parent) {
            parent = createParent(list, item.parent);//获取到新添加或已存在的项
            list = parent.childrens;
        }
        //if (list == null)
        //	debugger;
        var children = list.first(function (p) { return p.Id == item.Id });//first方法是申明在CommonJS.js里面的Array的扩展方法
        if (children == null) {//判断list里是否已有这一项
            var newItem = {};
            for (var pn in item) {
                newItem[pn] = item[pn];
            }
            newItem.childrens = [];
            newItem.parent = parent;
            list.push(newItem);
            sortList(list);
            return newItem;//创建新项做为下一项的父项返回
        }

        return children;//将已有项做为下一项的父项返回
    }

    function removeChildren(list, item) {
        if (item.parent) {
            item.parent.childrens.remove(item);//remove方法是申明在CommonJS.js里面的Array的扩展方法
            if (item.parent.childrens.length == 0)
                removeChildren(list, item.parent);
        }
        else
            list.remove(item);
    }

    function sortList(list) {
        list.sort(function (a, b) {
            return a.Id - b.Id;
        });
    }

    function showSelect(list) {
        var array = [];
        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            var temp = { name: item.Name, deep: 0 };
            if (item.isFull() == false) {
                temp.childrens = showSelect(item.childrens);
                var max = 0;
                for (var j = 0; j < temp.childrens.length; j++) {
                    if (temp.childrens[j].deep > max)
                        max = temp.childrens[j].deep;
                }
                temp.deep = max + 1;
            }
            array.push(temp);
        }
        return array;
    }

    function setUnit() {
        if ($('#radioPiece').attr('checked') == 'checked') {
            $('span[name="ValuationUnitDesc"]').text('件');
            $('span[name="ValuationUnit"]').html('件');
        }
        if ($('#radioWeight').attr('checked') == 'checked') {
            $('span[name="ValuationUnitDesc"]').text('重');
            $('span[name="ValuationUnit"]').html('kg');
        }
        if ($('#radioBulk').attr('checked') == 'checked') {
            $('span[name="ValuationUnitDesc"]').text('体积');
            $('span[name="ValuationUnit"]').html('m<sup>3</sup>');
        }
    }

    function SetFreeStatus() {
        if ($('#radioSelfDef').attr('checked') == 'checked') {
            $('#divContent').show();
            $("#divContent2").show();
        }
        else {
            $('#divContent').hide();
            $("#divContent2").hide();
            $("#ckbSpecifications").attr("checked", false);
            shippingfreeData.freightDetails = [];//清空数组集合
        }
    }

    function checkData(freightArea) {
        var inputTempNameTips = $("#inputTempNameTips");
        inputTempNameTips.html("");
        $('#inputTempName').removeClass("input-validation-error")
        if ($('#inputTempName').val() == '') {
            $('#inputTempName').addClass("input-validation-error").focus();
            inputTempNameTips.html("请输入运费模板名称");
            $.dialog.errorTips('请输入运费模板名称');
            return false;
        }
        if ($('#inputTempName').val().length>20) {
            $('#inputTempName').addClass("input-validation-error").focus();
            inputTempNameTips.html("运费模板名称不能超过20个字");
            $.dialog.errorTips('运费模板名称不能超过20个字');
            return false;
        }
        var sourceAddress = $('#SourceAddress').val();
        if (sourceAddress == '' || sourceAddress == '0') {
            $.dialog.errorTips('请选择商品地址');
            return false;
        }

        if ($('#radioSelfDef').attr('checked') == 'checked') {
            //默认运费检查
            var reg = /^[0-9]+([.]{1}[0-9]{1,3})?$/;
            var defFirstUnit = $('#inputDefFirstUnit').val(),
				defFirstUnitMonry = $('#inputDefFirstUnitMonry').val(),
				defAccumulationUnit = $('#inputDefAccumulationUnit').val(),
				defAccumulationUnitMoney = $('#inputDefAccumulationUnitMoney').val();
            if (!reg.test($('#inputDefFirstUnit').val()) || !reg.test($('#inputDefFirstUnitMonry').val()) || !reg.test($('#inputDefAccumulationUnit').val()) || !reg.test($('#inputDefAccumulationUnitMoney').val())) {
                $.dialog.errorTips('默认运费为空或不为数字，请检查');
                return false;
            }
            else {
                if (parseFloat(defFirstUnit) <= 0 || parseFloat(defFirstUnitMonry) < 0 || parseFloat(defAccumulationUnit) <= 0 || parseFloat(defAccumulationUnitMoney) < 0) {
                    $.dialog.errorTips('默认运费门槛必须大于0，请检查');
                    return false;
                }
                if (parseInt(defFirstUnit) != defFirstUnit) {
                    $.dialog.errorTips('默认计量只能为整数');
                    return false;
                }
                if (parseInt(defAccumulationUnit) != defAccumulationUnit) {
                    $.dialog.errorTips('增加计量只能为整数');
                    return false;
                }
            }

            var hasError = false;
            function checkValue(tr, selector, name, value) {
                $(selector, tr).addClass('error');

                if ($.isNullOrEmpty(value)) {
                    $.dialog.errorTips(name + '不能为空');
                    return false;
                }

                var temp = parseFloat(value);

                if (isNaN(temp)) {
                    $.dialog.errorTips(name + '不是数字');
                    return false;
                }
                //指定可配送区域的首费和续费都可以为 0
                if (name.indexOf("费") < 0) {
                    if (temp <= 0) {
                        $.dialog.errorTips(name + '不能小于0');
                        return false;
                    }
                    if (parseInt(temp) != temp) {
                        $.dialog.errorTips(name + '只能是整数');
                        return false;
                    }
                }

                $(selector, tr).removeClass('error');
                return true;
            }

            $('table.table-area-freight tbody tr').each(function (i) {
                if (hasError == true)
                    return;

                var tr = $(this);
                var item = {
                    firstUnit: $('.firstUnit', tr).val(),
                    firstUnitMonry: $('.firstUnitMonry', tr).val(),
                    accumulationUnit: $('.accumulationUnit', tr).val(),
                    accumulationUnitMoney: $('.accumulationUnitMoney', tr).val(),
                    isDefault: 0,
                    freightAreaDetail: []
                };

                if (!checkValue(tr, '.firstUnit', '首件', item.firstUnit) ||
					!checkValue(tr, '.firstUnitMonry', '首费', item.firstUnitMonry) ||
					!checkValue(tr, '.accumulationUnit', '续件', item.accumulationUnit) ||
					!checkValue(tr, '.accumulationUnitMoney', '续费', item.accumulationUnitMoney)) {
                    hasError = true;
                    return;
                }

                var selectArea = ulAreaData.rightProvice[i];
                if (selectArea == null || selectArea.length == 0) {
                    $.dialog.errorTips('运送地区不能为空');
                    hasError = true;
                    return;
                }

                for (var i = 0; i < selectArea.length; i++) {
                    var province = selectArea[i];
                    if (province.childrens.length == 0) {
                        item.freightAreaDetail.push({ provinceId: province.Id });
                        continue;
                    }
                    for (var j = 0; j < province.childrens.length; j++) {
                        var city = province.childrens[j];
                        if (city.childrens.length == 0) {
                            item.freightAreaDetail.push({ provinceId: province.Id, cityId: city.Id });
                            continue;
                        }
                        for (var k = 0; k < city.childrens.length; k++) {
                            var county = city.childrens[k];
                            item.freightAreaDetail.push({
                                provinceId: province.Id,
                                cityId: city.Id,
                                countyId: county.Id,
                                townIds: county.childrens.newitem(function (p) { return p.Id; }).join(',')
                            });
                        }
                    }
                }

                freightArea.push(item);
            });

            if (hasError == false)
                return freightArea;
            return false;
        }
        return true;
    }
    function checkFreeTempContentData(freeTempContent) {
        var hasError = false;
        function checkValue(tr, selector, name, value) {
            $(selector, tr).addClass('error');

            if ($.isNullOrEmpty(value)) {
                $.dialog.errorTips(name + '不能为空');
                return false;
            }

            var temp = parseFloat(value);

            if (isNaN(temp)) {
                $.dialog.errorTips(name + '不是数字');
                return false;
            }
            //指定可配送区域的首费和续费都可以为 0
            if (name.indexOf("费") < 0) {
                if (temp <= 0) {
                    $.dialog.errorTips(name + '不能小于0');
                    return false;
                }
                if (parseInt(temp) != temp) {
                    $.dialog.errorTips(name + '只能是整数');
                    return false;
                }
            }

            $(selector, tr).removeClass('error');
            return true;
        }
        //指定地区包邮,遍历所有行
        var conditionNumber = 0;
        $('table.table-shippingfree tbody tr').each(function (i) {
            if (hasError == true)
                return;
            var tr = $(this);
            var item = {
                ConditionType: parseFloat(tr.find('.setfreeshipping').val() || 0),
                ConditionNumber: '',
                FreightAreaDetail: []//指定地区包邮
            };
            var conditions = tr.find("input");//
            var condition1 = parseFloat(conditions.eq(0).val() || 0);
            var condition2 = parseFloat(conditions.eq(1).val() || 0);
            conditionNumber = condition1;
            if (conditions.length > 1)
                conditionNumber += "$" + condition2;
            item.ConditionNumber = conditionNumber;

            var selectArea = ulAreaData2.rightProvice[i];
            if (selectArea == null || selectArea.length == 0) {
                $.dialog.errorTips('运送地区不能为空');
                hasError = true;
                return;
            }

            if (!checkValue(tr, '.ConditionType', '包邮条件类型', item.ConditionType)) {
                hasError = true;
                return;
            }
            if (!checkValue(tr, '.ConditionNumber', '包邮条件值', condition1)) {
                hasError = true;
                return;
            }
            if (conditions.length > 1) {
                if (!checkValue(tr, '.ConditionNumber', '包邮条件值', condition2)) {
                    hasError = true;
                    return;
                }
            }
            for (var i = 0; i < selectArea.length; i++) {
                var province = selectArea[i];
                if (province.childrens.length == 0) {
                    item.FreightAreaDetail.push({ regionId: province.Id, regionPath: province.Id });
                    continue;
                }
                for (var j = 0; j < province.childrens.length; j++) {
                    var city = province.childrens[j];
                    if (city.childrens.length == 0) {
                        item.FreightAreaDetail.push({ regionId: city.Id, regionPath: province.Id + ',' + city.Id });
                        continue;
                    }
                    for (var k = 0; k < city.childrens.length; k++) {
                        var county = city.childrens[k];
                        item.FreightAreaDetail.push({
                            provinceId: province.Id,
                            cityId: city.Id,
                            countyId: county.Id,
                            regionId: county.Id,
                            regionPath: province.Id + ',' + city.Id + ',' + county.Id,
                            townIds: county.childrens.newitem(function (p) { return p.Id; }).join(',')
                        });
                    }
                }
            }
            freeTempContent.push(item);
        });

        if (hasError == false)
            return freeTempContent;
        return false;
    }

    function SaveData() {
        var freightArea = [{
            firstUnit: $('#inputDefFirstUnit').val(),
            firstUnitMonry: $('#inputDefFirstUnitMonry').val(),
            accumulationUnit: $('#inputDefAccumulationUnit').val(),
            accumulationUnitMoney: $('#inputDefAccumulationUnitMoney').val(),
            isDefault: 1,
            freightAreaDetail: []
        }];
        var freeTempContent = [];

        if (checkData(freightArea)) {
            var freightTemplate = {};
            freightTemplate.id = templateId;
            freightTemplate.Name = $('#inputTempName').val();
            freightTemplate.SourceAddress = $("#SourceAddress").val();
            freightTemplate.SendTime = $("#selsendtime").val();
            freightTemplate.IsFree = $('input[name="isfree"]:checked').val();
            freightTemplate.ValuationMethod = $('input[name="valuationMethod"]:checked').val();
            freightTemplate.FreightArea = freightArea;

            if ($("#ckbSpecifications").is(":checked")) {
                if (!checkFreeTempContentData(freeTempContent)) {
                    return;
                }
            }
            freightTemplate.freeTempContent = freeTempContent;
            console.log(freightTemplate);
            var loading = showLoading();
            $.post('SaveTemplate', { templateinfo: freightTemplate }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.succeedTips('保存成功！');
                    if (window.location.href.toLowerCase().indexOf('tar=freighttemplate') > 0) {
                        if (window.opener && window.opener.BindfreightTemplate) {
                            window.opener.BindfreightTemplate();
                        }
                        window.close();
                    }
                    window.location.href = "/SellerAdmin/FreightTemplate/Index";
                } else {
                    $.dialog.errorTips('保存失败！');
                }
            });
        }
    }

    window.freightTemplateAddJs = {
        addItem: addItem,
        addItem2: addItem2,
        removeItem: removeItem,
        removeItem2: removeItem2,
        selectItem: selectItem,
        openChildren: openChildren,
        openChildren2: openChildren2
    };

    function regionFreeChange() {
        //包邮选择框事件
        $('#regionFree').on('change', '.setfreeshipping', function () {
            $(this).next().empty();
            switch ($(this).val()) {
                case '1':
                    $(this).next().append('满<input type="text" value="" class="form-control mlr" style="width: 80px;display: inline-block;">件包邮');
                    break;
                case '2':
                    $(this).next().append('满<input type="text" value="" class="form-control mlr" style="width: 80px;display: inline-block;">元包邮');
                    break;
                case '3':
                    $(this).next().append('满<input type="text" value="" class="form-control mlr" style="width: 80px;display: inline-block;">件，<input type="text" value="" class="form-control" style="width: 80px;display: inline-block;"> 元包邮');
                    break;
                default:
                    return;
            }
        });
    }
})();