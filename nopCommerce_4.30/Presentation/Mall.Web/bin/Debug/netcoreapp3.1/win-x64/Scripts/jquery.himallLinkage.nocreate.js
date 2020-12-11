(function ($) {

    function getData(url, level, key) {
        var data;
        if (level == -1 || key) {
            $.ajax({
                url: url,
                async: false,
                data: { level: level, key: key },
                type: "post",
                dataType: "json",
                success: function (returnData) {
                    data = returnData;
                }
            });
        }
        else
            data = [];
        return data;
    }

    var selectors = {};
    var uid = 0;

    var selectedItems = {};

    var MallLinkageOptions = {};

    function setDefaultItem(index) {
        if (MallLinkageOptions[index].enableDefaultItem) {
            if (!$.isArray(MallLinkageOptions[index].defaultItemsValue)) {
                var arr = [];
                var defaultVallue = MallLinkageOptions[index].defaultItemsValue;
                if (defaultVallue == null)
                    defaultVallue = '';
                var i = MallLinkageOptions[index].level;
                while (i--) arr.push(defaultVallue);
                MallLinkageOptions[index].defaultItemsValue = arr;
            }
            else if (MallLinkageOptions[index].defaultItemsValue.length < MallLinkageOptions[index].level) {
                var less = MallLinkageOptions[index].level - MallLinkageOptions[index].defaultItemsValue.length;
                while (less--)
                    MallLinkageOptions[index].defaultItemsValue.push('');
            }

            if (!$.isArray(MallLinkageOptions[index].defaultItemsText)) {
                var arr = [];
                var defaultText = MallLinkageOptions[index].defaultItemsText;
                if (defaultText == null)
                    defaultText = '请选择';
                var i = MallLinkageOptions[index].level;
                while (i--) arr.push(defaultText);
                MallLinkageOptions[index].defaultItemsText = arr;
            }
            else {
                var itemLength = MallLinkageOptions[index].defaultItemsText.length;
                if (itemLength < MallLinkageOptions[index].level) {
                    var less = MallLinkageOptions[index].level - itemLength;
                    while (less--)
                        MallLinkageOptions[index].defaultItemsText.push('请选择');
                }
            }
        }
    }


    function clear(startIndex, index) {
        for (var i = startIndex; i < selectors[index].length; i++) {
            if (selectors[index][i]) {
                if (MallLinkageOptions[index].displayWhenNull)
                    selectors[index][i].empty().attr('disabled', 'disabled');
                else
                    selectors[index][i].empty().hide();
            }
        }
    }


    function drawSelect(level, key, index) {
        var newLevel = level + 1;
        var selector = selectors[index][newLevel];
        if (key == MallLinkageOptions[index].defaultItemsValue[level])
            clear(newLevel,index);

        if (MallLinkageOptions[index].displayWhenNull)
            selector.empty().removeAttr('disabled');
        else
            selector.empty().show();
        var data = getData(MallLinkageOptions[index].url, level, key);
        if (data != null && data.length > 0) {
            if (MallLinkageOptions[index].enableDefaultItem) {
                var text = '<option ';
                if (MallLinkageOptions[index].defaultItemsValue[newLevel])
                    text += ' value="' + MallLinkageOptions[index].defaultItemsValue[newLevel] + '"';
                else
                    text += ' value=""';
                text += '>' + MallLinkageOptions[index].defaultItemsText[newLevel] + '</option>';
                selector.append(text);
            }
            $.each(data, function (i, item) {
                selector.append('<option value="' + (item.key ? item.key : item.Key) + '">' + (item.value ? item.value : item.Value) + '</option>');
            });

            selector.unbind('change').change(function (item) {
                var currentIndex = parseInt($(this).attr('linkageId'));
                selectedItems[currentIndex][newLevel] = $(this).val();
                if (newLevel < MallLinkageOptions[currentIndex].level - 1)
                    drawSelect(newLevel, $(this).val(), currentIndex);
                if (MallLinkageOptions[currentIndex].onChange)
                    MallLinkageOptions[currentIndex].onChange(newLevel, $(this).val(), $(this).find('option:selected').text());
            });
        }
        else
            clear(newLevel, index);
        var currentIndex = parseInt(selector.attr('linkageId'));
        if (newLevel < MallLinkageOptions[currentIndex].level -1 )
            drawSelect(newLevel,selector.val(), currentIndex);
    }

    function selectValue(level, key, index) {
        var selector = selectors[index][level];
        selector.val(key);
        if (level + 1 < MallLinkageOptions[index].level)
            drawSelect(level, key, index);
    }

    function initDefaultSelectedValues(index) {
        var selectedValues = MallLinkageOptions[index].defaultSelectedValues;
        if (selectedValues && selectedValues.length > 0 && parseInt(selectedValues[0])) {
            selectedValues = [0].concat(selectedValues);
            MallLinkageOptions[index].defaultSelectedValues = selectedValues;
        }

        for (var i = 1; i < selectedValues.length; i++)
            selectValue(i - 1, selectedValues[i], index);
    }


    $.fn.MallLinkage = function (options, params) {
        /// <param name="params" type="object">$.fn.MallLinkage.options</param>
        if (typeof options == "string") {
            return $.fn.MallLinkage.methods[options](this, params);
        }
        resetData(uid);
        selectors[uid] = [];
        $.each($(this), function (i, item) {
            $(item).attr('linkageId', uid);
            selectors[uid].push($(item));
        })
        $.fn.MallLinkage.options = $.extend({}, $.fn.MallLinkage.options, options);
        $.fn.MallLinkage.options.level = selectors[uid].length;
        MallLinkageOptions[uid] = $.fn.MallLinkage.options;
        setDefaultItem(uid);
        drawSelect(-1, null, uid);
        initDefaultSelectedValues(uid);
        uid++;
        return $;
    }

    $.fn.MallLinkage.options = {
        url: null,//调用地址
        enableDefaultItem: false,//是否显示默认项（即未选中时的项）
        defaultItemsText: [],//默认文本，可以是数组，也可以是统一的值
        defaultItemsValue: [],//默认值，可以是数组，也可以是统一的值
        onChange: null,//select 的change事件
        displayWhenNull: true,//
        defaultSelectedValues:[]
    };


    $.fn.MallLinkage.methods = {
        value: function (jquery, level) {
            var index = parseInt($(jquery).attr('linkageId'));
            return selectedItems[index][level];
        },
        reset: function (jquery) {
            var index =  parseInt($(jquery).attr('linkageId'));
            drawSelect(-1, null, index);
        }
    }

    function resetData(index) {
        selectors[index] = [];
        selectedItems[index] = [];
    }

})(jQuery);