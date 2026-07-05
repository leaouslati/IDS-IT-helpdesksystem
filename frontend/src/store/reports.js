import { defineStore } from "pinia";
import api from "../api/axios";

export const useReportsStore = defineStore("reports", {
  state: () => ({
    pdfExporting:   false,
    excelExporting: false,
  }),

  actions: {
    async exportPdf(from, to) {
      this.pdfExporting = true;
      try {
        const res = await api.get("/reports/export/pdf", {
          params:       { from, to },
          responseType: "blob",
        });
        triggerDownload(res.data, `HelpDesk_Report_${from}_${to}.pdf`, "application/pdf");
      } finally {
        this.pdfExporting = false;
      }
    },

    async exportExcel(from, to) {
      this.excelExporting = true;
      try {
        const res = await api.get("/reports/export/excel", {
          params:       { from, to },
          responseType: "blob",
        });
        triggerDownload(
          res.data,
          `HelpDesk_Report_${from}_${to}.xlsx`,
          "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
      } finally {
        this.excelExporting = false;
      }
    },
  },
});

function triggerDownload(blob, filename, mimeType) {
  const url = URL.createObjectURL(new Blob([blob], { type: mimeType }));
  const a   = document.createElement("a");
  a.href     = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
