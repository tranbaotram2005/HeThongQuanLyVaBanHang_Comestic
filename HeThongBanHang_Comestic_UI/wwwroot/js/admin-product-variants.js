(function ($) {
    "use strict";

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, "&amp;")
            .replace(/"/g, "&quot;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;");
    }

    function parseCommaList(str) {
        var seen = {};
        var out = [];
        (str || "").split(",").forEach(function (part) {
            var t = part.trim();
            if (t && !seen[t.toLowerCase()]) {
                seen[t.toLowerCase()] = true;
                out.push(t);
            }
        });
        return out;
    }

    function syncOptionColumns() {
        var n1 = ($("#opt1Name").val() || "").trim();
        var n2 = ($("#opt2Name").val() || "").trim();

        $("#thOpt1Label").text(n1 || "Thuộc tính 1");
        $("#thOpt2Label").text(n2 || "Thuộc tính 2");

        if (n1.length > 0) $(".col-opt1").show();
        else $(".col-opt1").hide();

        if (n2.length > 0) $(".col-opt2").show();
        else $(".col-opt2").hide();
    }

    function findPresetByName(name) {
        if (!name || !window.__optionPresetsData) return null;
        var target = name.trim().toLowerCase();
        return window.__optionPresetsData.find(function (p) {
            return (p.name || "").trim().toLowerCase() === target;
        }) || null;
    }

    function renderPresetChips(slot) {
        var nameInput = $("#opt" + slot + "Name");
        var valuesInput = $("#opt" + slot + "ValuesInput");
        var chipsContainer = $("#opt" + slot + "PresetChips");
        var selectEl = $("#opt" + slot + "PresetSelect");

        var optName = (nameInput.val() || "").trim();
        var preset = findPresetByName(optName);

        if (preset) {
            selectEl.val(preset.name);
        } else {
            selectEl.val("");
        }

        chipsContainer.empty();
        if (!preset || !preset.values || preset.values.length === 0) {
            return;
        }

        var selectedList = parseCommaList(valuesInput.val());
        var selectedMap = {};
        selectedList.forEach(function (v) { selectedMap[v.toLowerCase()] = true; });

        var label = $('<span class="text-muted small mr-2 mb-1">Gợi ý nhanh:</span>');
        chipsContainer.append(label);

        preset.values.forEach(function (val) {
            var isActive = !!selectedMap[val.toLowerCase()];
            var btn = $('<button type="button" class="btn btn-xs btn-sm mr-1 mb-1 preset-chip-btn"></button>')
                .text((isActive ? "✓ " : "+ ") + val)
                .attr("data-slot", slot)
                .attr("data-val", val)
                .css({ padding: "1px 8px", fontSize: "0.78rem", borderRadius: "14px" })
                .addClass(isActive ? "btn-info" : "btn-outline-secondary");
            chipsContainer.append(btn);
        });

        var selectAllBtn = $('<button type="button" class="btn btn-link btn-sm p-0 ml-1 mb-1 preset-select-all" style="font-size:0.78rem;">Chọn tất cả</button>')
            .attr("data-slot", slot);
        chipsContainer.append(selectAllBtn);
    }

    function reindexVariantRows() {
        $("#variantTableBody tr.variant-row").each(function (idx) {
            $(this).find("input, select, textarea").each(function () {
                var name = $(this).attr("name");
                if (!name) return;
                if (name.indexOf("VariantFile_") === 0) {
                    $(this).attr("name", "VariantFile_" + idx);
                } else {
                    $(this).attr("name", name.replace(/Variants\[\d+\]/g, "Variants[" + idx + "]"));
                }
            });
        });
    }

    function appendVariantRow(data) {
        data = data || {};
        var isEditMode = $("#variantTable").attr("data-is-edit") === "true";
        var idx = $("#variantTableBody tr.variant-row").length;
        var extraHidden = isEditMode
            ? '<input type="hidden" name="Variants[' + idx + '].OriginalStatus" value="1" /><input type="hidden" name="Variants[' + idx + '].Status" value="1" />'
            : "";

        var rowHtml = `
            <tr class="variant-row">
                <td>
                    <input type="hidden" name="Variants[${idx}].Id" value="0" />
                    ${extraHidden}
                    <input type="text" name="Variants[${idx}].Name" value="${escapeHtml(data.name || "")}" class="form-control form-control-sm variant-name-input" placeholder="VD: 50ml - Hoa Hồng" />
                </td>
                <td class="col-opt1" style="display:none;">
                    <input type="text" name="Variants[${idx}].Option1Value" value="${escapeHtml(data.opt1 || "")}" class="form-control form-control-sm variant-opt1-input" placeholder="Giá trị nhóm 1" />
                </td>
                <td class="col-opt2" style="display:none;">
                    <input type="text" name="Variants[${idx}].Option2Value" value="${escapeHtml(data.opt2 || "")}" class="form-control form-control-sm variant-opt2-input" placeholder="Giá trị nhóm 2" />
                </td>
                <td>
                    <input type="text" name="Variants[${idx}].Sku" class="form-control form-control-sm variant-sku-input" placeholder="Tự tạo nếu trống" />
                </td>
                <td>
                    <input type="number" name="Variants[${idx}].Price" value="${data.price || ""}" min="0" step="1000" class="form-control form-control-sm variant-price-input" placeholder="0" />
                </td>
                <td>
                    <input type="number" name="Variants[${idx}].CompareAtPrice" min="0" step="1000" class="form-control form-control-sm" placeholder="Không bắt buộc" />
                </td>
                <td>
                    <input type="number" name="Variants[${idx}].StockQuantity" value="${data.stock !== undefined ? data.stock : ""}" min="0" class="form-control form-control-sm variant-stock-input" placeholder="0" />
                </td>
                <td>
                    <input type="file" name="VariantFile_${idx}" class="form-control-file form-control-sm variant-file-input" accept="image/*" />
                </td>
                <td class="text-center align-middle">
                    <input type="checkbox" name="Variants[${idx}].IsDefault" value="true" class="variant-default-chk" ${data.isDefault ? "checked" : ""} />
                </td>
                <td class="text-center align-middle">
                    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-variant" title="Xóa dòng">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>`;
        $("#variantTableBody").append(rowHtml);
        syncOptionColumns();
    }

    $(function () {
        $(document).on("change", ".preset-select", function () {
            var slot = $(this).attr("data-slot");
            var chosen = $(this).val() || "";
            $("#opt" + slot + "Name").val(chosen);
            syncOptionColumns();
            renderPresetChips(slot);
        });

        $(document).on("input", "#opt1Name, #opt2Name", function () {
            syncOptionColumns();
            renderPresetChips(1);
            renderPresetChips(2);
        });

        $(document).on("input", "#opt1ValuesInput", function () { renderPresetChips(1); });
        $(document).on("input", "#opt2ValuesInput", function () { renderPresetChips(2); });

        $(document).on("click", ".preset-chip-btn", function () {
            var slot = $(this).attr("data-slot");
            var val = $(this).attr("data-val");
            var input = $("#opt" + slot + "ValuesInput");
            var list = parseCommaList(input.val());
            var idx = list.findIndex(function (x) { return x.toLowerCase() === val.toLowerCase(); });
            if (idx >= 0) {
                list.splice(idx, 1);
            } else {
                list.push(val);
            }
            input.val(list.join(", "));
            renderPresetChips(slot);
        });

        $(document).on("click", ".preset-select-all", function () {
            var slot = $(this).attr("data-slot");
            var name = ($("#opt" + slot + "Name").val() || "").trim();
            var preset = findPresetByName(name);
            if (!preset || !preset.values) return;
            $("#opt" + slot + "ValuesInput").val(preset.values.join(", "));
            renderPresetChips(slot);
        });

        $("#btnAddVariantRow").on("click", function () {
            appendVariantRow();
        });

        $("#btnGenerateCombos").on("click", function () {
            var n1 = ($("#opt1Name").val() || "").trim();
            var n2 = ($("#opt2Name").val() || "").trim();
            var v1List = parseCommaList($("#opt1ValuesInput").val());
            var v2List = parseCommaList($("#opt2ValuesInput").val());

            if (v1List.length === 0 && v2List.length === 0) {
                alert("Vui lòng nhập hoặc bấm chọn giá trị cho Nhóm 1 hoặc Nhóm 2.");
                return;
            }
            if (v1List.length > 0 && !n1) {
                alert("Vui lòng chọn hoặc nhập Tên nhóm 1 (ví dụ: Dung tích).");
                $("#opt1Name").focus();
                return;
            }
            if (v2List.length > 0 && !n2) {
                alert("Vui lòng chọn hoặc nhập Tên nhóm 2 (ví dụ: Mùi hương).");
                $("#opt2Name").focus();
                return;
            }

            var combos = [];
            if (v1List.length > 0 && v2List.length > 0) {
                v1List.forEach(function (a) {
                    v2List.forEach(function (b) {
                        combos.push({ opt1: a, opt2: b, name: a + " - " + b });
                    });
                });
            } else if (v1List.length > 0) {
                v1List.forEach(function (a) {
                    combos.push({ opt1: a, opt2: "", name: a });
                });
            } else {
                v2List.forEach(function (b) {
                    combos.push({ opt1: "", opt2: b, name: b });
                });
            }

            var isEditMode = $("#variantTable").attr("data-is-edit") === "true";
            var rows = $("#variantTableBody tr.variant-row");
            if (!isEditMode && rows.length === 1) {
                var firstRow = rows.first();
                var isRowEmpty = !firstRow.find(".variant-name-input").val().trim()
                    && !firstRow.find(".variant-opt1-input").val().trim()
                    && !firstRow.find(".variant-opt2-input").val().trim()
                    && !firstRow.find(".variant-sku-input").val().trim()
                    && !firstRow.find(".variant-price-input").val().trim();
                if (isRowEmpty) {
                    firstRow.remove();
                }
            }

            var existingKeys = {};
            $("#variantTableBody tr.variant-row").each(function () {
                var o1 = ($(this).find(".variant-opt1-input").val() || "").trim().toLowerCase();
                var o2 = ($(this).find(".variant-opt2-input").val() || "").trim().toLowerCase();
                existingKeys[o1 + "||" + o2] = true;
            });

            var hasDefault = $(".variant-default-chk:checked").length > 0;
            var basePrice = $("#UnitPrice").val() || "";

            combos.forEach(function (c, i) {
                var key = c.opt1.toLowerCase() + "||" + c.opt2.toLowerCase();
                if (existingKeys[key]) return;
                existingKeys[key] = true;

                var makeDefault = !hasDefault && i === 0;
                if (makeDefault) hasDefault = true;

                appendVariantRow({
                    name: c.name,
                    opt1: c.opt1,
                    opt2: c.opt2,
                    price: basePrice,
                    stock: 0,
                    isDefault: makeDefault
                });
            });

            reindexVariantRows();
            syncOptionColumns();
        });

        $(document).on("click", ".btn-remove-variant", function () {
            var isEditMode = $("#variantTable").attr("data-is-edit") === "true";
            if (!isEditMode && $("#variantTableBody tr.variant-row").length <= 1) {
                $(this).closest("tr").find('input[type="text"], input[type="number"], input[type="file"]').val("");
                return;
            }
            $(this).closest("tr").remove();
            reindexVariantRows();
        });

        $(document).on("change", ".variant-default-chk", function () {
            if (this.checked) {
                $(".variant-default-chk").not(this).prop("checked", false);
            }
        });

        syncOptionColumns();
        renderPresetChips(1);
        renderPresetChips(2);
    });
})(jQuery);
