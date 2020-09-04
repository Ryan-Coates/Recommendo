
<template>
    <div id="recommendations">
        <h1>{{type}}</h1>
        <recomendation-item
        v-for='recommendation in recommendations'
        :key='recommendation.id'
        :recommendation='recommendation'
      >
      </recomendation-item>
      <br><hr>
      <recomendation-form @addRecommendation='appendRecommendation' :type=type></recomendation-form>
    </div>
</template>
<script>
import RecomendationItem from './RecommendationItem.vue'
import RecomendationForm from './RecommendationForm.vue'

export default {
  name: 'Recommendations',
  components: {
    RecomendationItem,
    RecomendationForm
  },
  props: [
    'recommendations',
    'type'
  ],
  methods: {
    appendRecommendation (name, description, recommender) {
      this.recommendations.push({ name: name, description: description, recommender: recommender })
    }

  }
}
</script>
