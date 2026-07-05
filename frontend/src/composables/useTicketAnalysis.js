import { ref } from "vue";
import { aiApi } from "../api/aiApi";

export function useTicketAnalysis() {
  const isAnalyzing = ref(false);
  const analysisResult = ref(null);
  const analysisError = ref("");

  async function analyze(description) {
    isAnalyzing.value = true;
    analysisError.value = "";
    analysisResult.value = null;
    try {
      const response = await aiApi.analyzeTicket(description);
      analysisResult.value = response.data;
    } catch {
      analysisError.value = "AI analysis unavailable — please select manually";
    } finally {
      isAnalyzing.value = false;
    }
  }

  return { isAnalyzing, analysisResult, analysisError, analyze };
}
