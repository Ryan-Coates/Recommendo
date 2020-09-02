<template>
    <li>
        {{recommendation.name}}: {{recommendation.description}}
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
