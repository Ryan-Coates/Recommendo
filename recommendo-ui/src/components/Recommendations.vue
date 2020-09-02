
<template>
    <div id="recommendations">
        <h1>{{ type }}</h1>
        <recomendation-item
        v-for='recommendation in recommendations'
        :key='recommendation.id'
        :recommendation='recommendation'
      >{{recommendation.name}}:{{recommendation.description}}
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
  data () {
    return {
      title: 'All Recomendations',
      recommendations: [
        { name: 'Self-Reliance', description: 'Ralph Waldo Emerson' },
        { name: 'American Gods', description: 'Neil Gaiman' },
        { name: 'Amusing Ourselves to Death', description: 'Neil Postman' }
      ]
    }
  },
  props: {
    type: String
  },
  methods: {
    appendRecommendation (name, description) {
      this.recommendations.push({ name: name, description: description })
    }

  }
}
</script>
