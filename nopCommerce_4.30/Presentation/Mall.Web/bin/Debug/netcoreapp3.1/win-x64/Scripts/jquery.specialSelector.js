Array.prototype.contains = function (obj) {
    var flag = false;
    this.forEach(function (item, i) {
        if (item == obj) {
            flag = true;
            return flag;
        }
    });
    return flag;
};
$.specialSelector = {
    params: { selectedProductIds: [], specialName: '', exceptProductIds: [] },
    serviceType: 'admin',
    multiSelect: true,
    selectedProducts: [],
    html: '<div id="_productSelector" class="goods-choose clearfix">\
            <div class="choose-left">\
                <div class="choose-search">\
                    <div class="form-group" >\
                        <input class="form-control input-ssm" type="text" id="" placeholder="请输入专题名称" >\
                    </div>\
                    <button type="button" class="btn btn-primary btn-ssm">搜索</button>\
                </div>\
                <table class="table table-bordered table-choose"></table>\
            </div>\
            \
        </div>\
',
    operationButtons: null,
    loadedProducts: null,
    reload: function (selectedProductIds, exceptProductIds) {
        this.loadedProducts = [];
        var url = '/Admin/PageSettings/GetSpecial';

        $.specialSelector.params.selectedProductIds = selectedProductIds;
        $.specialSelector.params.exceptProductIds = exceptProductIds;
        var columns = [
             { checkbox: true, width: 50 },
                {
                    field: "Name", title: '专题名称', width: 366, align: "left"
                },
            {
                field: "Tags", title: "专题标签", width: 100, align: "center"
            },
            {
                field: "s", title: "操作", width: 86, align: "center",
                formatter: function (value, row, index) {
                    $.specialSelector.loadedProducts[row.Id.toString()] = row;
                    var html = '<span class="btn-a" productId="' + row.Id + '">';
                    if ($.specialSelector.params.selectedProductIds && $.specialSelector.params.selectedProductIds.indexOf(row.Id) > -1)
                        html += '已选择';
                    else
                        html += '<a productId="' + row.Id + '" href="javascript:;" onclick="$.specialSelector.select(' + row.Id + ',this)" class="active" >选择';
                    html += '</a></span>';
                    return html;
                },
                styler: function () {
                    return 'td-operate';
                }
            }
        ];

        var start, end;
        var newColumns = [];
        if (this.multiSelect) {
            start = 1;
            end = columns.length;
        }
        else {
            start = 0;
            end = columns.length - 1;
        }
        for (var i = start; i < end; i++) {
            newColumns.push(columns[i]);
        }
        columns = newColumns;


        $("#_productSelector table").MallDatagrid({
            url: url,
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            hasCheckbox: !this.multiSelect,
            singleSelect: !this.multiSelect,
            idField: "Id",
            pageSize: 8,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: this.params,
            operationButtons: this.operationButtons,
            columns: [columns]
        });

        $("#_productSelector .choose-search button").unbind('click').click(function () {
            var specialName = $("#_productSelector .choose-search input").val();
            specialName = $.trim(specialName);
            $.specialSelector.params.specialName = specialName;
            $("#_productSelector table").MallDatagrid('reload', $.specialSelector.params);
        });

    },
    select: function (productId, sender, needScroll) {
        var product = this.loadedProducts[productId];
        this.selectedProducts[productId] = product;
        if (!this.params.selectedProductIds)
            this.params.selectedProductIds = [];
        this.params.selectedProductIds.push(productId);
        var span = $(sender).parent();
        $(sender).remove();
        span.html('已选择');
    },
    //选择当前页所有未选择的商品
    selectAll: function () {
        var _this = this;
        $('#_productSelector table a.active[productId]').each(function () {
            var pid = $(this).attr('productId');
            if (_this.params.selectedProductIds.contains(pid))
                return;

            _this.select(pid, this, false);
        });
    },
    removeProduct: function (productId) {
        $('#_productSelector ul li[productId="' + productId + '"]').remove();
        var removedProducts = [];
        var productIds = [];

        var selectedProds = this.selectedProducts;
        $.each(this.params.selectedProductIds, function (i, id) {
            if (id && id != productId) {
                removedProducts[id] = selectedProds[id];
                productIds.push(id);
            }
        });

        this.selectedProducts = removedProducts;
        this.params.selectedProductIds = productIds;
        var btn = $('span[productId="' + productId + '"]');
        if (btn) {
            btn.html(
            '<a productId="' + productId + '" href="javascript:;" onclick="$.specialSelector.select(' + productId + ',this)" class="active" >选择');
            btn.addClass('active');
        }
    },
    clear: function () {
        this.selectedProducts = [];
        $("#_productSelector ul").empty();
        this.params = { specialName: '', selectedProductIds: [] };
    },
    getSelected: function () {
        var products = [];
        if (this.multiSelect) {
            //$.each(this.selectedProducts, function (i, product) {
            //    if (product)
            //        products.push(product);
            //});
            //按选择的顺序返回，原来是根据商品ID来排序
            var selectedProds = this.selectedProducts;
            $.each(this.params.selectedProductIds, function (i, id) {
                if (id && selectedProds[id]) {
                    products.push(selectedProds[id]);
                }
            });
        }
        else {
            products.push($("#_productSelector table").MallDatagrid('getSelected'));
        }
        return products;
    },
    show: function (selectedProductIds, onSelectFinishedCallBack, serviceType, multiSelect, exceptProductIds, operationButtons) {
        if (serviceType)
            this.serviceType = serviceType;
        if (multiSelect != null)
            this.multiSelect = multiSelect;
        this.operationButtons = operationButtons;

        $.dialog({
            title: '选择专题',
            lock: true,
            content: this.html,
            padding: '0',
            ok: function () {
                var result = onSelectFinishedCallBack && onSelectFinishedCallBack($.specialSelector.getSelected());
                if (result != false) {
                    $.specialSelector.clear();
                    $('#_productSelector').remove();
                }
                else {
                    return false;
                }
            },
            close: function () {
                $.specialSelector.clear();
                $('#_productSelector').remove();
            }
        });

        if (!this.multiSelect) {
            $('.choose-right').hide();
            $('.choose-left').css('width', '100%');
        }

        //注册删除事件
        $('#_productSelector').on('click', 'i[type="del"]', function () {
            var parent = $(this).parent();
            $.specialSelector.removeProduct(parent.attr('productId'));
        });
        this.clear();
        this.reload(selectedProductIds, exceptProductIds);
    }
};
