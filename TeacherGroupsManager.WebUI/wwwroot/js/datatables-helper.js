window.arabicDataTablesLanguage = {
    processing: 'جاري التحميل',
    search: 'بحث',
    lengthMenu: 'عرض _MENU_ سجل',
    info: 'عرض _START_ إلى _END_ من أصل _TOTAL_ سجل',
    infoEmpty: 'عرض 0 إلى 0 من أصل 0 سجل',
    infoFiltered: '(تمت التصفية من أصل _MAX_ سجل)',
    loadingRecords: 'جاري التحميل',
    zeroRecords: 'لا توجد نتائج مطابقة',
    emptyTable: 'لا توجد بيانات',
    paginate: {
        first: 'الأول',
        previous: 'السابق',
        next: 'التالي',
        last: 'الأخير'
    }
};

window.initServerDataTable = function (options) {
    const table = $(options.tableSelector).DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        ordering: true,
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        ajax: {
            url: options.url,
            type: 'POST',
            data: function (data) {
                data.filters = collectDataTableFilters(options.filters || {});
                const token = $('input[name="__RequestVerificationToken"]').first().val();
                if (token) {
                    data.__RequestVerificationToken = token;
                }
            }
        },
        columns: options.columns,
        language: window.arabicDataTablesLanguage,
        direction: 'rtl',
        autoWidth: false
    });

    $(options.searchButtonSelector).on('click', function () {
        table.ajax.reload();
    });

    $(options.resetButtonSelector).on('click', function () {
        Object.values(options.filters || {}).forEach(function (selector) {
            $(selector).val('');
        });
        table.search('');
        table.ajax.reload();
    });

    $(options.reloadButtonSelector).on('click', function () {
        table.ajax.reload(null, false);
    });

    return table;
};

window.collectDataTableFilters = function (filters) {
    const values = {};
    Object.keys(filters).forEach(function (key) {
        values[key] = $(filters[key]).val();
    });
    return values;
};

window.renderBooleanStatus = function (value) {
    return value ? 'نشط' : 'غير نشط';
};

window.renderDate = function (value) {
    if (!value) return '';
    return String(value).substring(0, 10);
};

window.renderMoney = function (value) {
    return Number(value || 0).toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};

window.renderGroupType = function (value) {
    return Number(value) === 1 ? 'درس خاص' : 'مجموعة عامة';
};

window.renderLessonType = function (value) {
    return Number(value) === 1 ? 'درس خاص' : 'درس للمجموعة بالكامل';
};

window.renderPaymentStatus = function (value) {
    switch (Number(value)) {
        case 3: return 'مدفوع';
        case 2: return 'مدفوع جزئي';
        default: return 'غير مدفوع';
    }
};

window.renderDayOfWeek = function (value) {
    const days = ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];
    return days[Number(value)] || '';
};

window.renderActions = function (controller, id, includeDetails, permissionsUrl) {
    const details = includeDetails ? `<a class="btn btn-sm btn-outline-secondary" href="/${controller}/Details/${id}">تفاصيل</a> ` : '';
    const permissions = permissionsUrl ? `<a class="btn btn-sm btn-outline-secondary" href="${permissionsUrl}/${id}">الصلاحيات</a> ` : '';
    return `<div class="text-nowrap">
        ${details}<a class="btn btn-sm btn-outline-primary" href="/${controller}/Edit/${id}">تعديل</a>
        ${permissions}<form action="/${controller}/Delete/${id}" method="post" class="d-inline">
            <input name="__RequestVerificationToken" type="hidden" value="${$('input[name="__RequestVerificationToken"]').first().val() || ''}" />
            <button class="btn btn-sm btn-outline-danger" type="submit" onclick="return confirm('هل تريد حذف هذا السجل؟')">حذف</button>
        </form>
    </div>`;
};
