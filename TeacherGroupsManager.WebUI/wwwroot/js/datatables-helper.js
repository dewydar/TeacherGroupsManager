(function () {
    const lang = (document.documentElement.lang || 'ar').toLowerCase();
    const current = lang.startsWith('fr') ? 'fr' : lang.startsWith('en') ? 'en' : 'ar';
    const messages = {
        ar: {
            processing: 'جاري التحميل',
            search: 'بحث',
            lengthMenu: 'عرض _MENU_ سجل',
            info: 'عرض _START_ إلى _END_ من أصل _TOTAL_ سجل',
            infoEmpty: 'عرض 0 إلى 0 من أصل 0 سجل',
            infoFiltered: '(تمت التصفية من أصل _MAX_ سجل)',
            loadingRecords: 'جاري التحميل',
            zeroRecords: 'لا توجد نتائج مطابقة',
            emptyTable: 'لا توجد بيانات',
            first: 'الأول',
            previous: 'السابق',
            next: 'التالي',
            last: 'الأخير',
            active: 'نشط',
            inactive: 'غير نشط',
            privateLesson: 'درس خاص',
            publicGroup: 'مجموعة عامة',
            groupLesson: 'درس للمجموعة بالكامل',
            paid: 'مدفوع',
            partiallyPaid: 'مدفوع جزئي',
            unpaid: 'غير مدفوع',
            details: 'تفاصيل',
            edit: 'تعديل',
            permissions: 'الصلاحيات',
            delete: 'حذف',
            confirmDelete: 'هل تريد حذف هذا السجل؟',
            locale: 'ar-EG',
            days: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']
        },
        en: {
            processing: 'Loading',
            search: 'Search',
            lengthMenu: 'Show _MENU_ records',
            info: 'Showing _START_ to _END_ of _TOTAL_ records',
            infoEmpty: 'Showing 0 to 0 of 0 records',
            infoFiltered: '(filtered from _MAX_ records)',
            loadingRecords: 'Loading',
            zeroRecords: 'No matching records found',
            emptyTable: 'No data available',
            first: 'First',
            previous: 'Previous',
            next: 'Next',
            last: 'Last',
            active: 'Active',
            inactive: 'Inactive',
            privateLesson: 'Private lesson',
            publicGroup: 'Public group',
            groupLesson: 'Full group lesson',
            paid: 'Paid',
            partiallyPaid: 'Partially paid',
            unpaid: 'Unpaid',
            details: 'Details',
            edit: 'Edit',
            permissions: 'Permissions',
            delete: 'Delete',
            confirmDelete: 'Do you want to delete this record?',
            locale: 'en-US',
            days: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
        },
        fr: {
            processing: 'Chargement',
            search: 'Rechercher',
            lengthMenu: 'Afficher _MENU_ enregistrements',
            info: 'Affichage de _START_ à _END_ sur _TOTAL_ enregistrements',
            infoEmpty: 'Affichage de 0 à 0 sur 0 enregistrement',
            infoFiltered: '(filtré depuis _MAX_ enregistrements)',
            loadingRecords: 'Chargement',
            zeroRecords: 'Aucun résultat correspondant',
            emptyTable: 'Aucune donnée disponible',
            first: 'Premier',
            previous: 'Précédent',
            next: 'Suivant',
            last: 'Dernier',
            active: 'Actif',
            inactive: 'Inactif',
            privateLesson: 'Leçon privée',
            publicGroup: 'Groupe public',
            groupLesson: 'Leçon pour tout le groupe',
            paid: 'Payé',
            partiallyPaid: 'Payé partiellement',
            unpaid: 'Non payé',
            details: 'Détails',
            edit: 'Modifier',
            permissions: 'Autorisations',
            delete: 'Supprimer',
            confirmDelete: 'Voulez-vous supprimer cet enregistrement ?',
            locale: 'fr-FR',
            days: ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi']
        }
    };
    const t = messages[current];

    window.appTexts = t;
    window.arabicDataTablesLanguage = {
        processing: t.processing,
        search: t.search,
        lengthMenu: t.lengthMenu,
        info: t.info,
        infoEmpty: t.infoEmpty,
        infoFiltered: t.infoFiltered,
        loadingRecords: t.loadingRecords,
        zeroRecords: t.zeroRecords,
        emptyTable: t.emptyTable,
        paginate: {
            first: t.first,
            previous: t.previous,
            next: t.next,
            last: t.last
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
            direction: document.documentElement.dir || 'rtl',
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
        return value ? t.active : t.inactive;
    };

    window.renderDate = function (value) {
        if (!value) return '';
        return String(value).substring(0, 10);
    };

    window.renderMoney = function (value) {
        return Number(value || 0).toLocaleString(t.locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    window.renderGroupType = function (value) {
        return Number(value) === 1 ? t.privateLesson : t.publicGroup;
    };

    window.renderLessonType = function (value) {
        return Number(value) === 1 ? t.privateLesson : t.groupLesson;
    };

    window.renderPaymentStatus = function (value) {
        switch (Number(value)) {
            case 3: return t.paid;
            case 2: return t.partiallyPaid;
            default: return t.unpaid;
        }
    };

    window.renderDayOfWeek = function (value) {
        return t.days[Number(value)] || '';
    };

    window.renderGroupSchedules = function (value, type, row) {
        const schedules = Array.isArray(row.schedules) && row.schedules.length ? row.schedules : [{
            dayOfWeek: row.dayOfWeek,
            startTime: row.startTime,
            endTime: row.endTime
        }];

        return schedules.map(function (schedule) {
            return `${window.renderDayOfWeek(schedule.dayOfWeek)} ${schedule.startTime} - ${schedule.endTime}`;
        }).join('<br>');
    };

    window.initGroupSchedulesForm = function (formSelector, tableSelector, addButtonSelector) {
        const $form = $(formSelector);
        const $table = $(tableSelector);
        const dayOptions = t.days.map(function (day, index) {
            return `<option value="${index}">${day}</option>`;
        }).join('');

        function reindexSchedules() {
            $table.find('tbody tr').each(function (index) {
                $(this).find('.schedule-day').attr('name', `Schedules[${index}].DayOfWeek`);
                $(this).find('.schedule-start').attr('name', `Schedules[${index}].StartTime`);
                $(this).find('.schedule-end').attr('name', `Schedules[${index}].EndTime`);
            });
            syncPrimarySchedule();
        }

        function syncPrimarySchedule() {
            const $first = $table.find('tbody tr:first');
            $('#PrimaryDayOfWeek').val($first.find('.schedule-day').val() || '6');
            $('#PrimaryStartTime').val($first.find('.schedule-start').val() || '18:00');
            $('#PrimaryEndTime').val($first.find('.schedule-end').val() || '20:00');
        }

        $(addButtonSelector).on('click', function () {
            $table.find('tbody').append(`<tr>
                <td><select class="form-select schedule-day">${dayOptions}</select></td>
                <td><input class="form-control schedule-start" type="time" value="18:00" /></td>
                <td><input class="form-control schedule-end" type="time" value="20:00" /></td>
                <td><button class="btn btn-outline-danger btn-sm remove-schedule" type="button">${t.delete}</button></td>
            </tr>`);
            reindexSchedules();
        });

        $table.on('click', '.remove-schedule', function () {
            if ($table.find('tbody tr').length <= 1) return;
            $(this).closest('tr').remove();
            reindexSchedules();
        });

        $table.on('change', '.schedule-day, .schedule-start, .schedule-end', syncPrimarySchedule);
        $form.on('submit', function () {
            reindexSchedules();
            syncPrimarySchedule();
        });
    };

    window.renderActions = function (controller, id, includeDetails, permissionsUrl) {
        const details = includeDetails ? `<a class="btn btn-sm btn-outline-secondary" href="/${controller}/Details/${id}">${t.details}</a> ` : '';
        const permissions = permissionsUrl ? `<a class="btn btn-sm btn-outline-secondary" href="${permissionsUrl}/${id}">${t.permissions}</a> ` : '';
        return `<div class="table-actions">
            ${details}<a class="btn btn-sm btn-outline-primary" href="/${controller}/Edit/${id}">${t.edit}</a>
            ${permissions}<form action="/${controller}/Delete/${id}" method="post" class="d-inline">
                <input name="__RequestVerificationToken" type="hidden" value="${$('input[name="__RequestVerificationToken"]').first().val() || ''}" />
                <button class="btn btn-sm btn-outline-danger" type="submit" onclick="return confirm('${t.confirmDelete}')">${t.delete}</button>
            </form>
        </div>`;
    };
})();
