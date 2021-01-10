<template>
    <li>
      Name: {{recommendation.name}} | Description: {{recommendation.description}} | Recommender: {{recommendation.recommender}}
    </li>
</template>
<script>
export default {
  name: 'RecommendationItem',
  props: [
    'recommendation'
  ]
}
</script>
<style>
li{
    display: block;
    margin: 0 10px;
}
</style>
